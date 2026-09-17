using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Traduzione italiana di Little Witch in the Woods, come mod runtime.
    /// Nessun file del gioco viene modificato: la lingua viene aggiunta in memoria
    /// all'avvio.
    ///
    /// Il momento in cui agganciarsi non e' scelto a caso. Il gioco decide la lingua
    /// all'avvio in <c>PlayerPrefsLocaleSelector.GetStartupLocale()</c>: quando quel
    /// metodo parte, le lingue sono gia' caricate ma **nessuna tabella di testo e'
    /// stata ancora richiesta**. E' l'unica finestra in cui si puo' registrare
    /// l'italiano e costruirne le tabelle senza infilare un'attesa sincrona dentro
    /// un caricamento gia' in corso.
    /// </summary>
    [BepInPlugin(Id, "Little Witch in the Woods in italiano", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string Id = "liuknoceda.littlewitch.italiano";
        internal const string Codice = "it";
        internal const string CodiceModello = "en";   // lingua da cui si clona

        internal static Plugin Istanza;
        internal static ManualLogSource Registro;
        internal static Testi TestiCaricati;
        internal static Dialoghi DialoghiCaricati;
        internal static TabelleItaliane Tabelle;
        internal static bool GiaFatto;

        private void Awake()
        {
            Istanza = this;
            Registro = Logger;
            Registro.LogInfo("=== Little Witch in the Woods in italiano ===");

            string accanto = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestiCaricati = new Testi(Registro);
            Registro.LogInfo("Traduzioni:");
            TestiCaricati.Carica(Path.Combine(accanto, "traduzioni"));
            Registro.LogInfo($"  in tutto: {TestiCaricati.Voci} voci in {TestiCaricati.Collezioni} collezioni");

            DialoghiCaricati = new Dialoghi(Registro);
            DialoghiCaricati.Carica(Path.Combine(accanto, "traduzioni", "dialoghi"));

            new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
            Battito.Installa();
            Registro.LogInfo("Patch applicate, aspetto l'avvio della localizzazione");
        }

        /// <summary>La lingua italiana registrata, da usare quando servira'.</summary>
        internal static Locale Italiano;

        /// <summary>
        /// Registra la lingua italiana e ne costruisce le tabelle. Chiamata una volta
        /// sola, dal prefix su <c>GetStartupLocale</c>.
        /// </summary>
        internal static void Prepara(ILocalesProvider lingue)
        {
            if (GiaFatto) return;
            GiaFatto = true;

            try
            {
                var modello = lingue.GetLocale(new LocaleIdentifier(CodiceModello));
                if (modello == null)
                {
                    Registro.LogError($"Lingua modello '{CodiceModello}' non trovata: non posso costruire l'italiano.");
                    return;
                }

                var italiano = lingue.GetLocale(new LocaleIdentifier(Codice));
                if (italiano == null)
                {
                    italiano = Locale.CreateLocale(new LocaleIdentifier(Codice));
                    italiano.name = "Italian (it)";
                    lingue.AddLocale(italiano);
                    Registro.LogInfo($"Lingua aggiunta: '{italiano.Identifier.Code}' ({italiano.LocaleName})");
                }

                // Le tabelle non si costruiscono qui: questo metodo gira dentro una
                // callback di Addressables, dove aspettare un caricamento fa scattare
                // «Reentering the Update method is not allowed». Il fornitore le
                // costruisce da se', quando il gioco le chiede.
                Tabelle = new TabelleItaliane(TestiCaricati, Codice, modello, Registro);

                // Da qui in poi ogni richiesta di tabella italiana passa da noi.
                LocalizationSettings.StringDatabase.TableProvider = Tabelle;
                LocalizationSettings.AssetDatabase.TableProvider = Tabelle;
                Registro.LogInfo("Fornitore di tabelle installato");

                // Gli accenti si sistemano dopo, da GancioAccenti: qui siamo dentro
                // una callback di Addressables e caricare font farebbe scattare
                // «Reentering the Update method is not allowed».
                Italiano = italiano;
            }
            catch (Exception e)
            {
                Registro.LogError("Preparazione fallita: " + e);
            }
        }
    }

    /// <summary>
    /// Il gancio d'avvio. Prima che il gioco legga da PlayerPrefs quale lingua
    /// caricare, ci infiliamo a registrare l'italiano: cosi' se la scelta salvata
    /// e' «it», <c>GetLocaleBySavedPrefs()</c> la trova invece di ripiegare.
    /// </summary>
    [HarmonyPatch(typeof(PlayerPrefsLocaleSelector), nameof(PlayerPrefsLocaleSelector.GetStartupLocale))]
    internal static class GancioAvvio
    {
        private static void Prefix(ILocalesProvider availableLocales)
        {
            Plugin.Registro.LogInfo("GetStartupLocale: preparo l'italiano");
            Plugin.Prepara(availableLocales);
        }
    }

    /// <summary>
    /// Fa caricare il database dei dialoghi **inglese** anche quando la lingua e'
    /// l'italiano: un `DialogueDB_it.asset` non esiste fra gli addressable del gioco,
    /// e senza questo scambio il caricamento fallirebbe lasciando il gioco muto.
    /// Il testo italiano viene poi innestato dentro quel database.
    /// </summary>
    [HarmonyPatch(typeof(SunnySideUp.DialogueSystemLocaleInitializer), "LoadAndRegisterLocalizedDatabase")]
    internal static class GancioDatabaseDialoghi
    {
        private static void Prefix(ref string code)
        {
            if (code != Plugin.Codice) return;
            Plugin.Registro.LogInfo($"[dialoghi] '{code}' non esiste come database: carico '{Plugin.CodiceModello}'");
            code = Plugin.CodiceModello;
        }
    }

    /// <summary>
    /// Innesta il testo italiano nel database appena caricato.
    ///
    /// Si aggancia dopo <c>Start()</c>, quando il database e' gia' stato passato a
    /// <c>DialogueManager.AddDatabase()</c>: si lavora su <c>masterDatabase</c>, che
    /// e' quello davvero consultato, e cosi' funziona anche se il gioco avesse un
    /// <c>initialDatabase</c> gia' assegnato invece di caricarlo da Addressables.
    /// </summary>
    [HarmonyPatch(typeof(SunnySideUp.DialogueSystemLocaleInitializer), "Start")]
    internal static class GancioTestiDialoghi
    {
        private static void Postfix()
        {
            try
            {
                if (DialoghiCaricati == null) return;
                DialoghiCaricati.Applica(PixelCrushers.DialogueSystem.DialogueManager.masterDatabase);
            }
            catch (Exception e)
            {
                Registro.LogError("[dialoghi] innesto fallito: " + e);
            }
        }

        private static ManualLogSource Registro => Plugin.Registro;
        private static Dialoghi DialoghiCaricati => Plugin.DialoghiCaricati;
    }

    /// <summary>
    /// Aggancia il font di ripiego per le vocali accentate.
    ///
    /// Il posto giusto non e' ovvio. Le coroutine del plugin non girano (l'oggetto
    /// BepInEx smette di ricevere <c>Update()</c>), e dentro le callback di
    /// Addressables non si puo' aspettare nulla. Serve un metodo del gioco che parta
    /// sul thread principale a stack pulito: <c>DialogueSystemLocaleInitializer.Start()</c>
    /// e' perfetto perche' fa gia' <c>WaitForCompletion()</c> per conto suo, il che
    /// dimostra che li' l'attesa sincrona e' lecita. Ha anche
    /// <c>[DefaultExecutionOrder(-1)]</c>, quindi arriva presto.
    /// </summary>
    [HarmonyPatch(typeof(SunnySideUp.DialogueSystemLocaleInitializer), "Start")]
    internal static class GancioAccenti
    {
        private static void Postfix()
        {
            try
            {
                Accenti.Sistema(Plugin.Italiano);
            }
            catch (Exception e)
            {
                Plugin.Registro.LogError("[accenti] fallito: " + e);
            }
        }
    }

    /// <summary>
    /// Mette l'italiano nel menu della lingua.
    ///
    /// <c>LocalizationSetter</c> non costruisce l'elenco dalle lingue disponibili:
    /// parte da <c>_localeNames</c>, un array serializzato nella scena che accoppia
    /// ogni <c>Locale</c> al <c>LocalizedString</c> col nome da mostrare, e ne tiene
    /// solo le voci il cui Locale risulta anche fra quelli disponibili. Registrare
    /// la lingua non basta, quindi: senza una voce qui dentro l'italiano resta
    /// invisibile.
    ///
    /// Va fatto in **Prefix**: <c>Awake()</c> legge l'array subito e ne ricava sia
    /// le etichette sia il dizionario degli indici, quindi dopo sarebbe tardi.
    /// </summary>
    [HarmonyPatch(typeof(SunnySideUp.LocalizationSetter), "Awake")]
    internal static class GancioMenuLingua
    {
        internal const string ChiaveNome = "Settings_Localization_Italian";
        internal const string NomeMostrato = "Italiano";

        private static void Prefix(SunnySideUp.LocalizationSetter __instance)
        {
            try
            {
                var campo = typeof(SunnySideUp.LocalizationSetter)
                    .GetField("_localeNames", BindingFlags.Instance | BindingFlags.NonPublic);
                if (!(campo?.GetValue(__instance) is Array array) || array.Length == 0)
                {
                    Plugin.Registro.LogWarning("[menu] _localeNames vuoto o assente: non posso aggiungere l'italiano");
                    return;
                }

                Type tipoVoce = array.GetType().GetElementType();
                var campoLocale = tipoVoce.GetField("Locale");
                var campoNome = tipoVoce.GetField("Name");

                var italiano = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(Plugin.Codice));
                if (italiano == null)
                {
                    Plugin.Registro.LogWarning("[menu] lingua italiana non registrata: niente da aggiungere");
                    return;
                }

                for (int i = 0; i < array.Length; i++)
                    if (campoLocale.GetValue(array.GetValue(i)) is Locale l && l.Identifier.Code == Plugin.Codice)
                    {
                        Plugin.Registro.LogInfo("[menu] voce italiana gia' presente");
                        return;
                    }

                // La tabella da cui prendere l'etichetta e' la stessa delle altre
                // lingue: si eredita il riferimento invece di indovinarlo.
                var modello = (LocalizedString)campoNome.GetValue(array.GetValue(0));
                var riferimentoTabella = modello.TableReference;

                // La chiave non esiste nel gioco: la si aggiunge alla tabella della
                // lingua in corso. «Italiano» si scrive uguale in tutte le lingue,
                // quindi va bene qualunque essa sia.
                var tabella = LocalizationSettings.StringDatabase.GetTable(riferimentoTabella);
                if (tabella == null)
                {
                    Plugin.Registro.LogWarning("[menu] tabella delle etichette non caricata: rimando");
                    return;
                }
                tabella.AddEntry(ChiaveNome, NomeMostrato);

                object voceNuova = Activator.CreateInstance(tipoVoce);
                campoLocale.SetValue(voceNuova, italiano);
                campoNome.SetValue(voceNuova, new LocalizedString(riferimentoTabella, ChiaveNome));

                Array allungato = Array.CreateInstance(tipoVoce, array.Length + 1);
                Array.Copy(array, allungato, array.Length);
                allungato.SetValue(voceNuova, array.Length);
                campo.SetValue(__instance, allungato);

                Plugin.Registro.LogInfo(
                    $"[menu] italiano aggiunto: da {array.Length} a {allungato.Length} voci " +
                    $"(etichetta '{NomeMostrato}' in tabella {tabella.TableCollectionName})");
            }
            catch (Exception e)
            {
                Plugin.Registro.LogError("[menu] aggiunta fallita: " + e);
            }
        }
    }
}
