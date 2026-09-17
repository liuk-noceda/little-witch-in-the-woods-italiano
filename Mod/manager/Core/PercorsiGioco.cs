using System.IO;

namespace LiukNoceda.LittleWitchManager.Core;

/// <summary>Tutti i percorsi che contano dentro l'installazione del gioco.</summary>
/// <remarks>
/// Un solo posto in cui e' scritto dove vanno le cose: se domani il plugin
/// cambiasse cartella, si cambia qui e non in cinque punti diversi.
/// </remarks>
public sealed class PercorsiGioco
{
    public string Gioco { get; }

    private PercorsiGioco(string gioco) => Gioco = gioco;

    public static PercorsiGioco Per(string gioco) => new(gioco);

    public string Eseguibile   => Path.Combine(Gioco, "LWIW.exe");
    public string Dati         => Path.Combine(Gioco, "LWIW_Data");
    public string Winhttp      => Path.Combine(Gioco, "winhttp.dll");
    public string BepInEx      => Path.Combine(Gioco, "BepInEx");
    public string Plugins      => Path.Combine(Gioco, "BepInEx", "plugins");
    public string Nostra       => Path.Combine(Gioco, "BepInEx", "plugins", "LiukNoceda");
    public string Dll          => Path.Combine(Nostra, "LittleWitchItalian.dll");
    public string Traduzioni   => Path.Combine(Nostra, "traduzioni");
    public string Dialoghi     => Path.Combine(Traduzioni, "dialoghi");
    public string ControllerPromptsDll => Path.Combine(Nostra, "ControllerPrompts.dll");
    public string Log          => Path.Combine(Gioco, "BepInEx", "LogOutput.log");

    /// <summary>Vero se questa cartella e' davvero Little Witch in the Woods.</summary>
    public bool GiocoValido => File.Exists(Eseguibile) && Directory.Exists(Dati);

    /// <summary>Vero se BepInEx risulta gia' installato (anche da un'altra mod).</summary>
    public bool BepInExPresente => File.Exists(Winhttp) && Directory.Exists(BepInEx);

    /// <summary>Vero se la nostra traduzione e' installata.</summary>
    public bool TraduzioneInstallata => File.Exists(Dll);

    /// <summary>Vero se il modulo prompt controller PlayStation e' installato.</summary>
    public bool ControllerPromptsInstallato => File.Exists(ControllerPromptsDll);

    /// <summary>
    /// I file che BepInEx aggiunge nella radice del gioco. Servono per toglierli
    /// tutti quando si disinstalla: nessuno di questi esisteva prima.
    /// </summary>
    public static readonly string[] FileDiBepInEx =
    {
        "winhttp.dll", "doorstop_config.ini", ".doorstop_version", "changelog.txt",
    };
}
