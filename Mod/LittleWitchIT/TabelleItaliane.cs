using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Fabbrica e fornisce a Unity Localization le tabelle della lingua italiana.
    ///
    /// Le tabelle italiane non esistono da nessuna parte: nei bundle del gioco ci
    /// sono solo inglese, coreano, giapponese e i due cinesi. Vengono costruite in
    /// memoria **clonando quelle inglesi** e sovrascrivendo i testi con i nostri.
    ///
    /// Clonare invece di partire da zero non e' pigrizia, e' necessario: ogni voce
    /// e' indirizzata da un **id numerico** che sta nel <see cref="SharedTableData"/>,
    /// non dal nome della chiave. Riusando lo stesso SharedData dell'inglese gli id
    /// restano quelli che il gioco si aspetta. Vale anche per le tabelle di asset
    /// (font, materiali, impostazioni di testo): clonando l'inglese, l'italiano
    /// eredita la grafica latina invece di restare senza font.
    ///
    /// L'aggancio e' <see cref="ITableProvider"/>, il punto di estensione pubblico
    /// che <c>LoadTableOperation</c> interroga **prima** di andare su Addressables.
    /// Se torniamo un handle non valido, il caricamento normale prosegue: cosi' le
    /// altre lingue non vengono toccate.
    ///
    /// **Niente attese sincrone qui dentro.** <c>ProvideTableAsync</c> viene chiamata
    /// dentro l'Update del ResourceManager, dove <c>WaitForCompletion()</c> lancia
    /// «Reentering the Update method is not allowed». Percio' non si aspetta la
    /// tabella inglese: si restituisce una **operazione a catena** che la clona
    /// quando sara' pronta. E' anche piu' economico, perche' costruisce solo le
    /// tabelle che il gioco chiede davvero.
    /// </summary>
    internal sealed class TabelleItaliane : ITableProvider
    {
        private readonly Dictionary<string, LocalizationTable> _pronte =
            new Dictionary<string, LocalizationTable>(StringComparer.Ordinal);

        private readonly Testi _testi;
        private readonly ManualLogSource _log;
        private readonly string _codice;
        private readonly Locale _modello;

        internal int Stringhe { get; private set; }
        internal int Sostituite { get; private set; }
        internal int SenzaTraduzione { get; private set; }

        internal TabelleItaliane(Testi testi, string codice, Locale modello, ManualLogSource log)
        {
            _testi = testi;
            _codice = codice;
            _modello = modello;
            _log = log;
        }

        // ------------------------------------------------------------------
        // ITableProvider
        // ------------------------------------------------------------------

        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(
            string nomeCollezione, Locale locale) where TTable : LocalizationTable
        {
            // Handle non valido = «non me ne occupo io», e il caricamento normale
            // da Addressables prosegue. E' cosi' che le altre lingue restano intatte.
            if (locale == null || locale.Identifier.Code != _codice || _modello == null)
                return default;

            // Gia' costruita in un giro precedente.
            if (_pronte.TryGetValue(nomeCollezione, out var gia) && gia is TTable pronta)
                return Addressables.ResourceManager.CreateCompletedOperation(pronta, null);

            try
            {
                if (typeof(TTable) == typeof(StringTable))
                    return Concatena<TTable, StringTable>(
                        LocalizationSettings.StringDatabase.GetTableAsync(nomeCollezione, _modello),
                        ClonaStringhe, nomeCollezione);

                if (typeof(TTable) == typeof(AssetTable))
                    return Concatena<TTable, AssetTable>(
                        LocalizationSettings.AssetDatabase.GetTableAsync(nomeCollezione, _modello),
                        ClonaAsset, nomeCollezione);
            }
            catch (Exception e)
            {
                _log.LogError($"Tabella '{nomeCollezione}' ({typeof(TTable).Name}): {e.Message}");
            }

            return default;
        }

        /// <summary>
        /// Aggancia la clonazione al caricamento della tabella inglese: quando quella
        /// e' pronta, la copia italiana viene costruita e restituita.
        /// </summary>
        private AsyncOperationHandle<TTable> Concatena<TTable, TConcreta>(
            AsyncOperationHandle<TConcreta> inglese,
            Func<TConcreta, string, TConcreta> clona,
            string nomeCollezione)
            where TTable : LocalizationTable
            where TConcreta : LocalizationTable
        {
            return Addressables.ResourceManager.CreateChainOperation(inglese, fatto =>
            {
                if (fatto.Status != AsyncOperationStatus.Succeeded || fatto.Result == null)
                {
                    _log.LogWarning($"Tabella inglese '{nomeCollezione}' non caricata: l'italiano non puo' clonarla");
                    return Addressables.ResourceManager.CreateCompletedOperation<TTable>(null, null);
                }

                var copia = clona(fatto.Result, nomeCollezione);
                _pronte[nomeCollezione] = copia;
                return Addressables.ResourceManager.CreateCompletedOperation(copia as TTable, null);
            });
        }

        // ------------------------------------------------------------------
        // Clonazione
        // ------------------------------------------------------------------

        private StringTable ClonaStringhe(StringTable originale, string collezione)
        {
            var traduzioni = _testi.PerCollezione(collezione);

            var copia = ScriptableObject.CreateInstance<StringTable>();
            copia.name = collezione + "_" + _codice;
            copia.SharedData = originale.SharedData;          // stessi id, obbligatorio
            copia.LocaleIdentifier = new LocaleIdentifier(_codice);

            int tradotte = 0, restate = 0;
            foreach (var coppia in originale)
            {
                var voce = coppia.Value;
                if (voce == null) continue;

                // Alcune voci non hanno un nome, solo l'id numerico. Si indirizzano
                // scrivendo «#id:<numero>» nel JSON, la stessa forma che usa
                // tools/01_estrai_testi.py quando estrae.
                string chiave = string.IsNullOrEmpty(voce.Key) ? "#id:" + coppia.Key : voce.Key;
                string testo = voce.LocalizedValue;
                Stringhe++;

                if (traduzioni != null &&
                    traduzioni.TryGetValue(chiave, out var tradotto) &&
                    !string.IsNullOrEmpty(tradotto))
                {
                    testo = tradotto;
                    Sostituite++;
                    tradotte++;
                }
                else
                {
                    // Mai lasciare vuoto: senza testo il gioco mostrerebbe la chiave
                    // grezza. Finche' la traduzione non c'e', si tiene l'inglese.
                    SenzaTraduzione++;
                    restate++;
                }

                var nuova = copia.AddEntry(coppia.Key, testo);
                // I metadati portano cose come SmartFormat: senza, i segnaposto
                // {0} smettono di essere sostituiti.
                if (nuova != null && voce.MetadataEntries != null)
                    foreach (var m in voce.MetadataEntries)
                        nuova.AddMetadata(m);
            }

            _log.LogInfo($"[tabella] {collezione}: {tradotte} tradotte, {restate} in inglese");
            return copia;
        }

        private AssetTable ClonaAsset(AssetTable originale, string collezione)
        {
            var copia = ScriptableObject.CreateInstance<AssetTable>();
            copia.name = collezione + "_" + _codice;
            copia.SharedData = originale.SharedData;
            copia.LocaleIdentifier = new LocaleIdentifier(_codice);

            int n = 0;
            foreach (var coppia in originale)
            {
                var voce = coppia.Value;
                if (voce == null) continue;
                // Stessa guid dell'inglese: l'italiano punta agli stessi asset,
                // cioe' ai font latini invece che a quelli coreani.
                copia.AddEntry(coppia.Key, voce.Guid);
                n++;
            }

            _log.LogInfo($"[asset]   {collezione}: {n} voci ereditate dall'inglese");
            return copia;
        }
    }
}
