using System;
using HarmonyLib;
using TMPro;
using UnityEngine.Localization.Settings;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Ganci per la traduzione di elementi dell'interfaccia non coperti dal sistema
    /// standard di Unity Localization:
    /// 1. Il giorno della settimana nell'HUD (orologio/datario in alto).
    /// 2. I titoli e testi delle missioni gestiti tramite TextTable e StringField di PixelCrushers.
    /// </summary>
    internal static class GanciGui
    {
        private static readonly AccessTools.FieldRef<SunnySideUp.TimeHUDController, TextMeshProUGUI> GetDateText =
            AccessTools.FieldRefAccess<SunnySideUp.TimeHUDController, TextMeshProUGUI>("_dateText");

        /// <summary>
        /// Traduce l'abbreviazione del giorno della settimana nell'HUD in tempo reale.
        /// Nel gioco base il giorno e' generato direttamente da DayOfWeek.ToString().Substring(0, 3).ToUpper()
        /// producendo "MON", "TUE", ecc. senza passare da alcuna tabella di localizzazione.
        /// </summary>
        [HarmonyPatch(typeof(SunnySideUp.TimeHUDController), "RefreshView")]
        internal static class GancioDataHUD
        {
            private static void Postfix(SunnySideUp.TimeHUDController __instance)
            {
                try
                {
                    if (LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                        return;

                    var textComp = GetDateText(__instance);
                    if (textComp == null || string.IsNullOrEmpty(textComp.text))
                        return;

                    string t = textComp.text;
                    if (t.Contains("MON")) t = t.Replace("MON", "LUN");
                    else if (t.Contains("Mon")) t = t.Replace("Mon", "Lun");
                    else if (t.Contains("TUE")) t = t.Replace("TUE", "MAR");
                    else if (t.Contains("Tue")) t = t.Replace("Tue", "Mar");
                    else if (t.Contains("WED")) t = t.Replace("WED", "MER");
                    else if (t.Contains("Wed")) t = t.Replace("Wed", "Mer");
                    else if (t.Contains("THU")) t = t.Replace("THU", "GIO");
                    else if (t.Contains("Thu")) t = t.Replace("Thu", "Gio");
                    else if (t.Contains("FRI")) t = t.Replace("FRI", "VEN");
                    else if (t.Contains("Fri")) t = t.Replace("Fri", "Ven");
                    else if (t.Contains("SAT")) t = t.Replace("SAT", "SAB");
                    else if (t.Contains("Sat")) t = t.Replace("Sat", "Sab");
                    else if (t.Contains("SUN")) t = t.Replace("SUN", "DOM");
                    else if (t.Contains("Sun")) t = t.Replace("Sun", "Dom");

                    textComp.text = t;
                }
                catch (Exception e)
                {
                    Plugin.Registro?.LogError("[hud] errore traduzione data: " + e);
                }
            }
        }

        /// <summary>
        /// Risolve le chiavi di localizzazione di TextTable (es. Main_Ellie_000_001_title)
        /// cercando nei file JSON di traduzione (QuestNode, QuestUI, ecc.).
        /// Se la chiave non e' tradotta in italiano, ripiega sul testo inglese
        /// invece di mostrare la chiave grezza a schermo.
        /// </summary>
        [HarmonyPatch(typeof(PixelCrushers.TextTable), nameof(PixelCrushers.TextTable.GetFieldTextForLanguage), new[] { typeof(int), typeof(string) })]
        internal static class GancioTextTableInt
        {
            private static bool Prefix(PixelCrushers.TextTable __instance, int fieldID, string language, ref string __result)
            {
                if (language != Plugin.Codice && LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                    return true;
                if (language != Plugin.Codice && !string.IsNullOrEmpty(language))
                    return true;

                string key = __instance.GetFieldName(fieldID);
                if (string.IsNullOrEmpty(key)) return true;

                if (Plugin.TestiCaricati != null && Plugin.TestiCaricati.Cerca(key, out string trad))
                {
                    __result = trad;
                    return false;
                }

                // Ripiego su lingua inglese se disponibile nel TextTable
                string en = __instance.GetFieldTextForLanguage(fieldID, Plugin.CodiceModello);
                if (!string.IsNullOrEmpty(en) && en != key)
                {
                    __result = en;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PixelCrushers.TextTable), nameof(PixelCrushers.TextTable.GetFieldTextForLanguage), new[] { typeof(string), typeof(string) })]
        internal static class GancioTextTableString
        {
            private static bool Prefix(PixelCrushers.TextTable __instance, string fieldName, string language, ref string __result)
            {
                if (language != Plugin.Codice && LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                    return true;
                if (language != Plugin.Codice && !string.IsNullOrEmpty(language))
                    return true;

                if (string.IsNullOrEmpty(fieldName)) return true;

                if (Plugin.TestiCaricati != null && Plugin.TestiCaricati.Cerca(fieldName, out string trad))
                {
                    __result = trad;
                    return false;
                }

                string en = __instance.GetFieldTextForLanguage(fieldName, Plugin.CodiceModello);
                if (!string.IsNullOrEmpty(en) && en != fieldName)
                {
                    __result = en;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PixelCrushers.TextTable), nameof(PixelCrushers.TextTable.GetFieldText), new[] { typeof(int) })]
        internal static class GancioTextTableGetFieldTextInt
        {
            private static bool Prefix(PixelCrushers.TextTable __instance, int fieldID, ref string __result)
            {
                if (LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                    return true;

                string key = __instance.GetFieldName(fieldID);
                if (string.IsNullOrEmpty(key)) return true;

                if (Plugin.TestiCaricati != null && Plugin.TestiCaricati.Cerca(key, out string trad))
                {
                    __result = trad;
                    return false;
                }

                string en = __instance.GetFieldTextForLanguage(fieldID, Plugin.CodiceModello);
                if (!string.IsNullOrEmpty(en) && en != key)
                {
                    __result = en;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PixelCrushers.TextTable), nameof(PixelCrushers.TextTable.GetFieldText), new[] { typeof(string) })]
        internal static class GancioTextTableGetFieldTextString
        {
            private static bool Prefix(PixelCrushers.TextTable __instance, string fieldName, ref string __result)
            {
                if (LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                    return true;

                if (string.IsNullOrEmpty(fieldName)) return true;

                if (Plugin.TestiCaricati != null && Plugin.TestiCaricati.Cerca(fieldName, out string trad))
                {
                    __result = trad;
                    return false;
                }

                string en = __instance.GetFieldTextForLanguage(fieldName, Plugin.CodiceModello);
                if (!string.IsNullOrEmpty(en) && en != fieldName)
                {
                    __result = en;
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Intercetta StringField.value per catturare qualsiasi titolo/obiettivo quest
        /// che possa essere valutato direttamente da StringField o salvato come testo grezzo.
        /// </summary>
        [HarmonyPatch(typeof(PixelCrushers.StringField), "get_value")]
        internal static class GancioStringField
        {
            private static void Postfix(PixelCrushers.StringField __instance, ref string __result)
            {
                if (LocalizationSettings.SelectedLocale?.Identifier.Code != Plugin.Codice)
                    return;
                if (Plugin.TestiCaricati == null)
                    return;

                if (!string.IsNullOrEmpty(__result) && Plugin.TestiCaricati.Cerca(__result, out string tradResult))
                {
                    __result = tradResult;
                }
                else if (!string.IsNullOrEmpty(__instance.text) && Plugin.TestiCaricati.Cerca(__instance.text, out string tradText))
                {
                    __result = tradText;
                }
            }
        }
    }
}
