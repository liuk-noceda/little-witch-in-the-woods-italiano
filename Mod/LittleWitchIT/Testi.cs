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
        }

        /// <summary>Traduzioni di una collezione, o null se non ne abbiamo.</summary>
        internal Dictionary<string, string> PerCollezione(string collezione)
        {
            return _perCollezione.TryGetValue(collezione, out var d) ? d : null;
        }
    }
}
