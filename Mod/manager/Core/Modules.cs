using System;
using System.Collections.Generic;
using System.IO;

namespace LiukNoceda.LittleWitchManager.Core;

public static class ModIds
{
    public const string TranslationPlugin = "liuknoceda.littlewitch.italiano";
    public const string PadPlugin = "liuknoceda.littlewitch.controllerprompts";
    public const string Folder = "LiukNoceda";
}

public enum ModuleId
{
    Translation,
    ControllerPrompts,
}

public sealed class Module
{
    public required ModuleId Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string PayloadFolder { get; init; }
    public required string MarkerFile { get; init; }
    public string TargetSubfolder { get; init; } = "";

    public static readonly Module Translation = new()
    {
        Id = ModuleId.Translation,
        Name = "Traduzione italiana",
        Description = "Traduce dialoghi, interfaccia, enciclopedia e descrizioni in italiano. "
                    + "Aggiunge la voce «Italiano» al selettore lingua senza rimuovere le altre.",
        PayloadFolder = "Traduzione",
        MarkerFile = "LittleWitchItalian.dll",
    };

    public static readonly Module ControllerPrompts = new()
    {
        Id = ModuleId.ControllerPrompts,
        Name = "Icone PlayStation (DualSense)",
        Description = "Mostra le icone PlayStation (Croce, Cerchio, Quadrato, Triangolo, L1/R1, L2/R2) "
                    + "per controller DualSense e Sony al posto di quelle Xbox. Usa la grafica ufficiale del gioco.",
        PayloadFolder = "ControllerPrompts",
        MarkerFile = "ControllerPrompts.dll",
    };

    public static readonly IReadOnlyList<Module> All = new[] { Translation, ControllerPrompts };

    public static Module Get(ModuleId id) => id switch
    {
        ModuleId.Translation => Translation,
        ModuleId.ControllerPrompts => ControllerPrompts,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    public string MarkerPath(PercorsiGioco p) =>
        Path.Combine(p.Nostra, TargetSubfolder, MarkerFile);

    public bool IsInstalled(PercorsiGioco p) => File.Exists(MarkerPath(p));
}
