using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace LiukNoceda.LittleWitchManager.Core;

/// <summary>
/// Applica e rimuove i moduli (Traduzione, Icone PlayStation).
/// Non modifica file originali del gioco.
/// </summary>
public static class Installatore
{
    public static string Carico =>
        Path.Combine(AppContext.BaseDirectory, "Payload");

    public static string CaricoBepInEx => Path.Combine(Carico, "BepInEx");

    public static bool PossoScrivere(string cartella)
    {
        var prova = Path.Combine(cartella, ".prova_" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllText(prova, "x");
            File.Delete(prova);
            return true;
        }
        catch { return false; }
    }

    public static bool GiocoAperto() =>
        Process.GetProcessesByName("LWIW").Length > 0;

    public static void AssicuraBepInEx(PercorsiGioco p, Action<string>? passo = null)
    {
        if (p.BepInExPresente)
        {
            passo?.Invoke("BepInEx già presente: lasciato com'è.");
            return;
        }

        if (!Directory.Exists(CaricoBepInEx))
            throw new DirectoryNotFoundException($"Cartella BepInEx mancante in {Carico}");

        passo?.Invoke("Installazione di BepInEx...");
        CopiaCartella(CaricoBepInEx, p.Gioco);
    }

    public static void InstallaModulo(string gioco, Module modulo, Action<string>? passo = null)
    {
        var p = PercorsiGioco.Per(gioco);
        if (!p.GiocoValido) throw new InvalidOperationException("Cartella del gioco non valida.");

        AssicuraBepInEx(p, passo);

        var src = Path.Combine(Carico, modulo.PayloadFolder);
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"Payload del modulo mancante: {src}");

        Directory.CreateDirectory(p.Nostra);
        var dst = string.IsNullOrEmpty(modulo.TargetSubfolder)
            ? p.Nostra
            : Path.Combine(p.Nostra, modulo.TargetSubfolder);
        Directory.CreateDirectory(dst);

        passo?.Invoke($"Installazione {modulo.Name}...");
        CopiaCartella(src, dst);
        passo?.Invoke($"{modulo.Name}: installato.");
    }

    public static void DisinstallaModulo(string gioco, Module modulo, Action<string>? passo = null)
    {
        var p = PercorsiGioco.Per(gioco);
        if (!p.GiocoValido) return;

        passo?.Invoke($"Rimozione {modulo.Name}...");

        if (modulo.Id == ModuleId.Translation)
        {
            if (File.Exists(p.Dll)) File.Delete(p.Dll);
            if (Directory.Exists(p.Traduzioni)) Directory.Delete(p.Traduzioni, true);
            RimettiInglese();
        }
        else if (modulo.Id == ModuleId.ControllerPrompts)
        {
            if (File.Exists(p.ControllerPromptsDll)) File.Delete(p.ControllerPromptsDll);
        }
        else
        {
            var f = modulo.MarkerPath(p);
            if (File.Exists(f)) File.Delete(f);
        }

        passo?.Invoke($"{modulo.Name}: rimosso.");

        // Se non ci sono più moduli nostri, rimuovi la cartella LiukNoceda
        if (Directory.Exists(p.Nostra) && !Module.All.Any(m => m.IsInstalled(p)))
        {
            try { Directory.Delete(p.Nostra, true); } catch { }
        }

        // Se non ci sono altre mod, togli anche BepInEx
        if (!AltreMod(gioco) && !Module.All.Any(m => m.IsInstalled(p)))
        {
            RimuoviBepInEx(gioco, passo);
        }
    }

    public static void RipristinaOriginale(string gioco, Action<string>? passo = null)
    {
        var p = PercorsiGioco.Per(gioco);
        if (!p.GiocoValido) return;

        passo?.Invoke("Ripristino configurazione originale...");
        RimettiInglese();

        if (Directory.Exists(p.Nostra))
        {
            try { Directory.Delete(p.Nostra, true); } catch { }
        }

        if (!AltreMod(gioco))
        {
            RimuoviBepInEx(gioco, passo);
        }

        passo?.Invoke("Gioco riportato allo stato originale.");
    }

    private static void RimuoviBepInEx(string gioco, Action<string>? passo = null)
    {
        var p = PercorsiGioco.Per(gioco);
        passo?.Invoke("Rimozione di BepInEx...");
        try { if (Directory.Exists(p.BepInEx)) Directory.Delete(p.BepInEx, true); } catch { }
        foreach (var nome in PercorsiGioco.FileDiBepInEx)
        {
            try
            {
                var f = Path.Combine(gioco, nome);
                if (File.Exists(f)) File.Delete(f);
            }
            catch { }
        }
    }

    public static bool AltreMod(string gioco)
    {
        var p = PercorsiGioco.Per(gioco);
        if (!Directory.Exists(p.Plugins)) return false;
        try
        {
            return Directory.EnumerateFileSystemEntries(p.Plugins)
                            .Any(e => !string.Equals(Path.GetFileName(e), "LiukNoceda",
                                                     StringComparison.OrdinalIgnoreCase));
        }
        catch { return true; }
    }

    public static void RimettiInglese()
    {
        if (!OperatingSystem.IsWindows()) return;
        var valore = Encoding.UTF8.GetBytes("en").Concat(new byte[] { 0 }).ToArray();
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey(
                @"Software\SunnySideUp\Little Witch In The Woods", writable: true);
            if (k == null) return;
            foreach (var nome in new[] { "Language_h3872303031",
                                         "SelectedLanguageCode_h729211379" })
            {
                if (k.GetValue(nome) != null)
                    k.SetValue(nome, valore, RegistryValueKind.Binary);
            }
        }
        catch { }
    }

    private static void CopiaCartella(string da, string a)
    {
        Directory.CreateDirectory(a);
        foreach (var d in Directory.GetDirectories(da, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(d.Replace(da, a));
        foreach (var f in Directory.GetFiles(da, "*", SearchOption.AllDirectories))
            File.Copy(f, f.Replace(da, a), true);
    }

    public static void ApriCartella(string percorso)
    {
        if (!Directory.Exists(percorso)) return;
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{percorso}\"")
            {
                UseShellExecute = true
            });
        }
        catch { }
    }
}
