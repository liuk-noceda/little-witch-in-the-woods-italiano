using UnityEngine;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Un oggetto nostro che riceve <c>Update()</c>, perche' quello del plugin no.
    ///
    /// In questo gioco l'oggetto creato da BepInEx esegue <c>Awake()</c> e poi smette
    /// di essere aggiornato: le patch Harmony restano attive (sono statiche) ma
    /// <c>Update()</c> non viene mai chiamato, e quindi non girano nemmeno le
    /// coroutine. Verificato con un contatore che non ha mai scritto una riga.
    ///
    /// Serve pero' un battito, perche' alcune cose non si possono fare ne' dentro le
    /// callback di Addressables (vietato aspettare) ne' agganciandosi a un metodo del
    /// gioco che parte troppo tardi: <c>DialogueSystemLocaleInitializer.Start()</c>,
    /// per esempio, scatta solo quando si entra in partita, non alla schermata del
    /// titolo. Questo componente sta su un GameObject creato da noi, marcato
    /// <c>DontDestroyOnLoad</c>, e fa da orologio.
    /// </summary>
    internal sealed class Battito : MonoBehaviour
    {
        private bool _primoDetto;

        internal static void Installa()
        {
            var oggetto = new GameObject("LiukNoceda.LittleWitchItaliano");
            DontDestroyOnLoad(oggetto);
            oggetto.hideFlags = HideFlags.HideAndDontSave;
            oggetto.AddComponent<Battito>();
            Plugin.Registro.LogInfo("Battito installato su un oggetto nostro");
        }

        private void Update()
        {
            if (!_primoDetto)
            {
                _primoDetto = true;
                Plugin.Registro.LogInfo("Battito: primo giro, l'orologio va");
            }

            // Appena la lingua italiana e' pronta, sistema i font. Una volta sola:
            // ci pensa Accenti.Fatto.
            if (!Accenti.Fatto && Plugin.Italiano != null)
            {
                try
                {
                    Accenti.Sistema(Plugin.Italiano);
                }
                catch (System.Exception e)
                {
                    Accenti.Fatto = true;
                    Plugin.Registro.LogError("[accenti] fallito: " + e);
                }
            }
        }
    }
}
