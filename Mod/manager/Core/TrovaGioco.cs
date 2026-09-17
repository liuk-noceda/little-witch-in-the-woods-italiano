using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace LiukNoceda.LittleWitchManager.Core;

/// <summary>Trova l'installazione del gioco senza chiedere niente all'utente.</summary>
public static class TrovaGioco
{
    /// <summary>Il primo percorso valido, oppure null.</summary>
    public static string? Cerca()
    {
        foreach (var c in Candidati())
        {
            if (string.IsNullOrWhiteSpace(c)) continue;
            try
            {
                if (PercorsiGioco.Per(c).GiocoValido) return Normalizza(c);
            }
            catch { /* percorso malformato: si prova il prossimo */ }
        }
        return null;
    }

    /// <summary>
    /// Il registro di Steam restituisce percorsi con le barre miste
    /// (<c>c:/program files (x86)/steam\steamapps\...</c>): funzionano, ma mostrati
    /// all'utente sembrano un errore. Qui si riportano alla forma di Windows.
    /// </summary>
    private static string Normalizza(string p)
    {
        try { return Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar); }
        catch { return p; }
    }

    /// <summary>Tutti i percorsi da provare, dal piu' attendibile al piu' generico.</summary>
    public static IEnumerable<string> Candidati()
    {
        // 1. Controlla Player.log: se il gioco e' gia' stato avviato, il log
        // riporta esattamente la cartella da cui e' partito.
        var daLog = DaPlayerLog();
        if (daLog != null) yield return daLog;

        // Nelle librerie di Steam non si indovina il nome della cartella: si
        // guardano tutte quelle dentro steamapps\common. Il nome della cartella
        // del gioco non e' documentato e cambia fra edizioni.
        foreach (var lib in LibrerieSteam())
        {
            var comune = Path.Combine(lib, "steamapps", "common");
            string[] sotto;
            try { sotto = Directory.GetDirectories(comune); }
            catch { continue; }
            foreach (var d in sotto) yield return d;
        }

        foreach (var g in CartelleGog()) yield return g;

        // installazioni sciolte nei posti consueti
        foreach (var radice in new[]
                 {
                     @"C:\Games", @"D:\Games", @"E:\Games",
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                 })
        {
            if (string.IsNullOrEmpty(radice)) continue;
            yield return Path.Combine(radice, "Little Witch in the Woods");
            yield return Path.Combine(radice, "LittleWitchInTheWoods");
        }
    }

    /// <summary>Cartelle libreria di Steam, dal registro e da libraryfolders.vdf.</summary>
    private static IEnumerable<string> LibrerieSteam()
    {
        var steam = PercorsoSteam();
        if (steam == null) yield break;

        yield return steam;

        var vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdf)) yield break;

        string testo;
        try { testo = File.ReadAllText(vdf); }
        catch { yield break; }

        // "path"		"D:\\SteamLibrary"
        foreach (Match m in Regex.Matches(testo, "\"path\"\\s*\"([^\"]+)\"",
                                          RegexOptions.IgnoreCase))
        {
            var p = m.Groups[1].Value.Replace(@"\\", @"\");
            if (!string.IsNullOrWhiteSpace(p)) yield return p;
        }
    }

    private static string? PercorsoSteam()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            foreach (var chiave in new[] { @"Software\Valve\Steam",
                                           @"Software\WOW6432Node\Valve\Steam" })
            {
                using var utente = Registry.CurrentUser.OpenSubKey(chiave);
                if (utente?.GetValue("SteamPath") is string p && Directory.Exists(p)) return p;

                using var macchina = Registry.LocalMachine.OpenSubKey(chiave);
                if (macchina?.GetValue("InstallPath") is string q && Directory.Exists(q)) return q;
            }
        }
        catch { }
        return null;
    }

    private static IEnumerable<string> CartelleGog()
    {
        if (!OperatingSystem.IsWindows()) yield break;

        var trovate = new List<string>();
        try
        {
            foreach (var radice in new[] { @"SOFTWARE\WOW6432Node\GOG.com\Games",
                                           @"SOFTWARE\GOG.com\Games" })
            {
                using var chiave = Registry.LocalMachine.OpenSubKey(radice);
                if (chiave == null) continue;
                foreach (var nome in chiave.GetSubKeyNames())
                {
                    using var gioco = chiave.OpenSubKey(nome);
                    if (gioco?.GetValue("path") is string p && !string.IsNullOrWhiteSpace(p))
                        trovate.Add(p);
                }
            }
        }
        catch { }

        foreach (var t in trovate) yield return t;
    }

    /// <summary>Legge il percorso del gioco dall'ultimo avvio registrato in Player.log.</summary>
    private static string? DaPlayerLog()
    {
        try
        {
            var utente = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var log = Path.Combine(utente, "AppData", "LocalLow", "SunnySideUp", "Little Witch in the Woods", "Player.log");
            if (!File.Exists(log)) return null;

            using var fs = new FileStream(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            for (int i = 0; i < 30; i++)
            {
                var riga = sr.ReadLine();
                if (riga == null) break;
                var m = Regex.Match(riga, @"Mono path\[0\] = '([^']+)[/\\]LWIW_Data[/\\]Managed'");
                if (m.Success)
                {
                    var dir = m.Groups[1].Value.Replace('/', '\\');
                    if (File.Exists(Path.Combine(dir, "LWIW.exe")))
                        return dir;
                }
            }
        }
        catch { }
        return null;
    }
}
