using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Fa in modo che le vocali accentate si vedano.
    ///
    /// Il gioco esce in inglese, coreano, giapponese e i due cinesi: nessuna di
    /// queste lingue ha bisogno di `à è é ì ò ù`, e infatti i due font della lingua
    /// inglese — `JejuHallasan` e `Handwriting-Regular` — non li contengono. Senza
    /// rimedio ogni «perché» diventa un rettangolo vuoto.
    ///
    /// Il rimedio piu' economico e' dentro il gioco stesso: i font cinesi e
    /// giapponesi (`思源黑体CN-Medium`, `濑户字体`) hanno **tutte** le accentate.
    /// Vengono agganciati come **ripiego** (`fallbackFontAssetTable`) ai font
    /// italiani: TMP li interroga solo per i caratteri che il font principale non
    /// ha, quindi il testo resta nel carattere del gioco e cambiano disegno le sole
    /// lettere accentate.
    ///
    /// Non e' la soluzione definitiva — le accentate stonano un po' con il resto —
    /// ma non ridistribuisce nessun font e non tocca nessun file. La versione
    /// definitiva e' `tools\06_accenta_font.py`, che compone le accentate dentro
    /// `JejuHallasan` usando i suoi stessi segni: manca solo il modo di far
    /// digerire a TMP un TTF che non sta in un bundle del gioco.
    ///
    /// **Tutto sincrono, di proposito.** La strada naturale sarebbe una coroutine,
    /// ma in questo gioco l'oggetto del plugin BepInEx smette di ricevere `Update()`
    /// (vedi CONTINUA_QUI.md), quindi le coroutine non girano. Si chiama da un
    /// punto del gioco in cui l'attesa sincrona e' gia' dimostrata sicura.
    /// </summary>
    internal static class Accenti
    {
        internal const string DaVerificare = "àèéìòùÀÈÉÌÒÙ";
        private const string TabellaFont = "Fonts";

        internal static bool Fatto;

        internal static void Sistema(Locale italiano)
        {
            if (Fatto || italiano == null) return;
            Fatto = true;

            var log = Plugin.Registro;
            log.LogInfo("[accenti] cerco i font della lingua italiana");

            var fontItaliani = Raccogli(italiano);
            log.LogInfo($"[accenti] font italiani: {fontItaliani.Count}");
            if (fontItaliani.Count == 0)
            {
                log.LogWarning("[accenti] nessun font per l'italiano: non posso agganciare il ripiego");
                return;
            }

            var bisognosi = new List<TMP_FontAsset>();
            foreach (var f in fontItaliani)
            {
                if (SaScrivere(f, out string mancanti))
                {
                    log.LogInfo($"[accenti] font '{f.name}': accentate a posto");
                    continue;
                }
                log.LogInfo($"[accenti] font '{f.name}': mancano {mancanti}");
                bisognosi.Add(f);
            }
            if (bisognosi.Count == 0)
            {
                log.LogInfo("[accenti] tutti i font sanno gia' scrivere in italiano");
                return;
            }

            // Cerca fra le altre lingue del gioco un font che le abbia tutte.
            TMP_FontAsset soccorso = null;
            foreach (var altra in LocalizationSettings.AvailableLocales.Locales)
            {
                if (altra == null || altra.Identifier.Code == italiano.Identifier.Code) continue;
                foreach (var c in Raccogli(altra))
                {
                    if (c == null || bisognosi.Contains(c)) continue;
                    if (SaScrivere(c, out string mancanti))
                    {
                        soccorso = c;
                        log.LogInfo($"[accenti] font di ripiego: '{c.name}' (lingua {altra.Identifier.Code})");
                        break;
                    }
                    log.LogInfo($"[accenti]   scartato '{c.name}' ({altra.Identifier.Code}): mancano {mancanti}");
                }
                if (soccorso != null) break;
            }

            if (soccorso == null)
            {
                log.LogError("[accenti] nessun font del gioco contiene le vocali accentate: "
                             + "serve per forza un font nostro");
                return;
            }

            foreach (var f in bisognosi)
            {
                if (f.fallbackFontAssetTable == null)
                    f.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!f.fallbackFontAssetTable.Contains(soccorso))
                {
                    f.fallbackFontAssetTable.Add(soccorso);
                    log.LogInfo($"[accenti] '{soccorso.name}' agganciato come ripiego di '{f.name}'");
                }
            }

            // Verifica: cercando anche nei ripieghi, ora le accentate devono esserci.
            foreach (var f in bisognosi)
            {
                bool ok = f.HasCharacters(DaVerificare, out uint[] _, searchFallbacks: true, tryAddCharacter: true);
                log.LogInfo($"[accenti] verifica '{f.name}': {(ok ? "ora le ha" : "ANCORA senza")}");
            }
        }

        /// <summary>
        /// Dice se un font puo' scrivere le accentate, e nel farlo gliele fa generare.
        ///
        /// Non si usa <c>HasCharacters</c>: questi font sono in modalita' **Dynamic**
        /// e partono con zero glifi precotti, quindi <c>HasCharacters</c> risponde
        /// «no» anche quando il TTF sotto ce le ha eccome. <c>TryAddCharacters</c>
        /// invece prova a generarle davvero dal TTF: se riesce, la risposta e' un si'
        /// che vale, e i glifi restano in cache per quando serviranno.
        /// </summary>
        private static bool SaScrivere(TMP_FontAsset font, out string mancanti)
        {
            try
            {
                return font.TryAddCharacters(DaVerificare, out mancanti);
            }
            catch (System.Exception e)
            {
                mancanti = "errore: " + e.Message;
                return false;
            }
        }

        /// <summary>Carica i TMP_FontAsset che la tabella «Fonts» associa a una lingua.</summary>
        private static List<TMP_FontAsset> Raccogli(Locale lingua)
        {
            var fuori = new List<TMP_FontAsset>();
            var log = Plugin.Registro;

            AssetTable tabella;
            try
            {
                tabella = LocalizationSettings.AssetDatabase.GetTable(TabellaFont, lingua);
            }
            catch (System.Exception e)
            {
                log.LogWarning($"[accenti] tabella '{TabellaFont}' di '{lingua.Identifier.Code}': {e.Message}");
                return fuori;
            }
            if (tabella == null) return fuori;

            foreach (var coppia in tabella)
            {
                try
                {
                    var font = tabella.GetAssetAsync<TMP_FontAsset>(coppia.Key).WaitForCompletion();
                    if (font != null && !fuori.Contains(font)) fuori.Add(font);
                }
                catch (System.Exception e)
                {
                    log.LogWarning($"[accenti]   voce {coppia.Key}: {e.Message}");
                }
            }
            return fuori;
        }
    }
}
