using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiukNoceda.LittleWitchManager.Core;

/// <summary>Esito della lettura del log del gioco.</summary>
public sealed record EsitoLog(bool Trovato, bool Partito, bool BattuteVuote,
                              string Riassunto, IReadOnlyList<string> Righe);

/// <summary>
/// Legge <c>BepInEx\LogOutput.log</c> e dice, in italiano, se la traduzione e'
/// stata caricata davvero.
/// </summary>
/// <remarks>
/// E' l'unico modo per sapere se l'innesto dei dialoghi ha funzionato: succede
/// dentro il gioco, e da fuori non si vede. Il plugin scrive apposta una riga
/// «rilettura: N battute controllate, nessuna vuota» quando e' andato tutto bene.
/// </remarks>
public static class Diagnostica
{
    private static readonly string[] Interessanti =
    {
        "[tabella]", "[dialoghi]", "[accenti]", "Lingua aggiunta", "lingue disponibili",
    };

    public static EsitoLog Leggi(string gioco)
    {
        var log = PercorsiGioco.Per(gioco).Log;
        if (!File.Exists(log))
        {
            return new EsitoLog(false, false, false,
                "Nessun log: avvia il gioco almeno una volta dopo aver installato.",
                Array.Empty<string>());
        }

        string[] tutte;
        try { tutte = File.ReadAllLines(log); }
        catch (IOException)
        {
            return new EsitoLog(true, false, false,
                "Il log e' in uso: chiudi il gioco e riprova.", Array.Empty<string>());
        }

        var righe = tutte
            .Where(r => Interessanti.Any(i => r.Contains(i, StringComparison.Ordinal)))
            .Select(Ripulisci)
            .ToList();

        if (righe.Count == 0)
        {
            return new EsitoLog(true, false, false,
                "Il log non contiene righe della traduzione: il plugin non e' partito. "
                + "Controlla di aver avviato il gioco dopo l'installazione.",
                Array.Empty<string>());
        }

        var vuote = righe.Any(r => r.Contains("risultano vuote", StringComparison.Ordinal));
        var riassunto = vuote
            ? "Il log segnala battute vuote: qualcosa non va nell'innesto dei dialoghi."
            : "Il plugin e' partito e il log non segnala problemi.";

        return new EsitoLog(true, true, vuote, riassunto, righe);
    }

    /// <summary>Toglie il prefisso di BepInEx, che ripete sempre le stesse cose.</summary>
    private static string Ripulisci(string riga)
    {
        var chiuso = riga.IndexOf(']');
        if (chiuso > 0 && chiuso + 1 < riga.Length) riga = riga[(chiuso + 1)..];
        return riga.Trim();
    }
}
