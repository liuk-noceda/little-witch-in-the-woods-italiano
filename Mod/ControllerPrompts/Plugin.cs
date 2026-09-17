using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using SunnySideUp;

namespace LiukNoceda.ControllerPrompts
{
    /// <summary>
    /// Mostra i prompt PlayStation (DualSense / DualShock 4) al posto di quelli Xbox.
    ///
    /// Little Witch in the Woods ha GIA' le icone PlayStation complete negli asset
    /// (/DualShockGamepad: Croce, Cerchio, Quadrato, Triangolo, L1, R1, L2, R2, ecc.).
    /// Questo modulo NON sostituisce nessuna immagine, ma agisce sul rilevamento del controller,
    /// permettendo a DualSense, pad via Bluetooth e altri controller Sony di usare i prompt corretti.
    /// </summary>
    [BepInPlugin(GUID, "PlayStation Controller Prompts", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "liuknoceda.littlewitch.controllerprompts";

        internal enum PromptMode
        {
            /// <summary>Sempre PlayStation quando viene usato un gamepad.</summary>
            AlwaysPlayStation,
            /// <summary>Rilevamento automatico (riconosce anche DualSense, DualShock, Wireless Controller Bluetooth).</summary>
            Auto,
            /// <summary>Sempre Xbox: utile per ripristinare le icone Xbox.</summary>
            AlwaysXbox,
        }

        private static ConfigEntry<PromptMode> _mode;
        internal static BepInEx.Logging.ManualLogSource Log;

        private static readonly string[] SonyNames =
        {
            "playstation", "dualshock", "dualsense", "sony",
            "wireless controller",   // nome standard Bluetooth di DualShock/DualSense in Windows
            "ps4", "ps5",
        };

        private void Awake()
        {
            Log = Logger;
            _mode = Config.Bind("Generale", "Mode", PromptMode.AlwaysPlayStation,
                "AlwaysPlayStation = mostra sempre le icone PlayStation per i controller.\n" +
                "Auto = rileva il controller (riconosce DualSense, DualShock, Wireless Controller).\n" +
                "AlwaysXbox = forza le icone Xbox.");

            var harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo($"Prompt controller PlayStation attivi. Modalita': {_mode.Value}");
        }

        internal static bool IsPlayStation(InputDevice device)
        {
            if (device == null || device is Keyboard || device is Mouse) return false;

            switch (_mode?.Value ?? PromptMode.AlwaysPlayStation)
            {
                case PromptMode.AlwaysPlayStation:
                    return device is Gamepad;
                case PromptMode.AlwaysXbox:
                    return false;
                default:
                    if (device is DualShockGamepad) return true;
                    return IsSonyPad(device);
            }
        }

        private static bool IsSonyPad(InputDevice device)
        {
            if (device == null) return false;
            try
            {
                string name = device.name;
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.ToLowerInvariant();
                    foreach (var n in SonyNames)
                        if (name.Contains(n)) return true;
                }

                string product = device.description.product;
                if (!string.IsNullOrEmpty(product))
                {
                    product = product.ToLowerInvariant();
                    foreach (var n in SonyNames)
                        if (product.Contains(n)) return true;
                }

                string mfr = device.description.manufacturer;
                if (!string.IsNullOrEmpty(mfr))
                {
                    mfr = mfr.ToLowerInvariant();
                    if (mfr.Contains("sony")) return true;
                }
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    /// Forza il control scheme a /DualShockGamepad quando il controller attivo e' PlayStation.
    /// </summary>
    [HarmonyPatch(typeof(KeyIconData), nameof(KeyIconData.GetControlScheme))]
    internal static class GancioControlScheme
    {
        private static bool Prefix(InputDevice device, ref string __result)
        {
            if (device is Keyboard)
            {
                __result = "/Keyboard";
                return false;
            }

            if (Plugin.IsPlayStation(device))
            {
                __result = "/DualShockGamepad";
                return false;
            }

            if (device is Gamepad)
            {
                __result = "/Gamepad";
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Invoca l'evento _onChangeDualShock4 al posto di _onChangeGamepad quando e' presente un pad PlayStation.
    /// </summary>
    [HarmonyPatch(typeof(InputDeviceChangeEventHandler), "OnControlSchemeChanged")]
    internal static class GancioDeviceChange
    {
        private static bool Prefix(InputDeviceChangeEventHandler __instance, InputDevice device)
        {
            if (device is Keyboard) return true;

            if (Plugin.IsPlayStation(device))
            {
                var evt = AccessTools.Field(typeof(InputDeviceChangeEventHandler), "_onChangeDualShock4")
                    ?.GetValue(__instance) as UnityEngine.Events.UnityEvent;
                evt?.Invoke();
                return false;
            }

            return true;
        }
    }
}
