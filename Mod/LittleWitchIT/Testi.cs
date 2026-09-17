using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Carica le traduzioni dai file JSON accanto alla DLL.
    ///
    /// Un file per collezione di tabella, con lo stesso nome che ha nel gioco:
    /// <c>traduzioni\UI.json</c>, <c>traduzioni\Item.json</c>, ... Dentro, un
    /// oggetto piatto {chiave: testo}. Le chiavi sono quelle degli asset
    /// «Shared Data», le stesse che estrae <c>tools\01_estrai_testi.py</c>.
    ///
    /// I file si leggono all'avvio: per provare una modifica non serve
    /// ricompilare il plugin, basta rilanciare il gioco.
    /// </summary>
    internal sealed class Testi
    {
        private readonly Dictionary<string, Dictionary<string, string>> _perCollezione =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _tutti =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly ManualLogSource _log;

        internal Testi(ManualLogSource log) => _log = log;

        internal int Collezioni => _perCollezione.Count;

        internal int Voci
        {
            get
            {
                int n = 0;
                foreach (var d in _perCollezione.Values) n += d.Count;
                return n;
            }
        }

        internal void Carica(string cartella)
        {
            _perCollezione.Clear();

            if (!Directory.Exists(cartella))
            {
                _log.LogWarning($"Cartella delle traduzioni assente: {cartella}");
                return;
            }

            foreach (string file in Directory.GetFiles(cartella, "*.json"))
            {
                string collezione = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var voci = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(file));
                    if (voci == null || voci.Count == 0)
                    {
                        _log.LogWarning($"  {collezione}: file vuoto, saltato");
                        continue;
                    }
                    _perCollezione[collezione] = voci;
                    _log.LogInfo($"  {collezione}: {voci.Count} voci");
                }
                catch (Exception e)
                {
                    _log.LogError($"  {collezione}: JSON illeggibile — {e.Message}");
                }
            }

            AggiornaTutti();
        }

        private void AggiornaTutti()
        {
            _tutti.Clear();

            // Prima tutte le collezioni generali
            foreach (var kv in _perCollezione)
            {
                if (kv.Key == "QuestNode" || kv.Key == "QuestUI") continue;
                foreach (var entry in kv.Value)
                    if (!string.IsNullOrEmpty(entry.Value))
                        _tutti[entry.Key] = entry.Value;
            }

            // Poi sovrascrivi con QuestUI
            if (_perCollezione.TryGetValue("QuestUI", out var qui))
                foreach (var entry in qui)
                    if (!string.IsNullOrEmpty(entry.Value))
                        _tutti[entry.Key] = entry.Value;

            // Infine QuestNode ha la massima priorità per le quest
            if (_perCollezione.TryGetValue("QuestNode", out var qn))
                foreach (var entry in qn)
                    if (!string.IsNullOrEmpty(entry.Value))
                        _tutti[entry.Key] = entry.Value;
        }

        /// <summary>Traduzioni di una collezione, o null se non ne abbiamo.</summary>
        internal Dictionary<string, string> PerCollezione(string collezione)
        {
            return _perCollezione.TryGetValue(collezione, out var d) ? d : null;
        }

        /// <summary>
        /// Cerca la traduzione di una chiave in tutte le collezioni caricate,
        /// con priorità a QuestNode e QuestUI (usate per le missioni).
        /// </summary>
        internal bool Cerca(string chiave, out string valore)
        {
            valore = null;
            if (string.IsNullOrEmpty(chiave)) return false;
            return _tutti.TryGetValue(chiave, out valore) && !string.IsNullOrEmpty(valore);
        }
    }
}

