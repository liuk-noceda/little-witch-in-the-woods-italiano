"""Assembla la versione **portatile** della traduzione.

E' il ripiego per chi non vuole installare niente: non richiede .NET, e' solo un
.bat che chiama uno script PowerShell. Il canale principale e' l'altro —
`Installer/build.ps1`, che produce il gestore con interfaccia e il setup .exe.

Mette insieme, in `_dist/Little Witch in the Woods in italiano/`:

    Installa la traduzione italiana.bat   il lanciatore da cliccare
    LEGGIMI.txt
    _installer/installer.ps1              la logica (UTF-8 con BOM: senza,
                                          PowerShell 5.1 storpia le accentate)
    _installer/contenuto/                 il plugin, i testi, BepInEx

e poi zippa tutto in `_dist/LittleWitchInTheWoods-italiano.zip`.

    python tools/30_crea_installer.py            costruisce cartella + zip
    python tools/30_crea_installer.py --no-zip   solo la cartella

Il plugin va compilato prima:  cd Mod/LittleWitchIT && dotnet build -c Release
"""
import glob
import os
import shutil
import sys
import zipfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

SORGENTE = os.path.join(lwiw.MOD, "installer")
DLL = os.path.join(lwiw.MOD, "LittleWitchIT", "bin", "Release", "netstandard2.1",
                   "LittleWitchItalian.dll")
DIST = os.path.join(lwiw.ROOT, "_dist")
NOME = "Little Witch in the Woods in italiano (portatile)"
LANCIATORE = "Installa la traduzione italiana.bat"


def copia_bom(sorgente, destinazione):
    """Ricopia un file di testo aggiungendo il BOM UTF-8.

    Windows PowerShell 5.1 legge come ANSI i file senza BOM: senza questo
    passaggio ogni accentata dell'installatore diventa spazzatura.
    """
    with open(sorgente, encoding="utf-8") as f:
        testo = f.read()
    with open(destinazione, "w", encoding="utf-8-sig", newline="\r\n") as f:
        f.write(testo)


def main():
    if not os.path.isfile(DLL):
        print(f"Plugin non compilato: {DLL}")
        print("Esegui prima:  cd Mod/LittleWitchIT && dotnet build -c Release")
        return 1

    bepinex = sorted(glob.glob(os.path.join(lwiw.ROOT, "_downloads", "BepInEx_win_x64_*.zip")))
    if not bepinex:
        print("Manca BepInEx in _downloads/: scarica BepInEx_win_x64_*.zip")
        return 1
    bepinex = bepinex[-1]

    radice = os.path.join(DIST, NOME)
    if os.path.isdir(radice):
        shutil.rmtree(radice)
    interno = lwiw.assicura(os.path.join(radice, "_installer"))
    contenuto = lwiw.assicura(os.path.join(interno, "contenuto"))

    # --- quello che l'utente vede ---
    copia_bom(os.path.join(SORGENTE, "avvia.bat"), os.path.join(radice, LANCIATORE))
    copia_bom(os.path.join(SORGENTE, "LEGGIMI.txt"), os.path.join(radice, "LEGGIMI.txt"))
    copia_bom(os.path.join(SORGENTE, "installer.ps1"),
              os.path.join(interno, "installer.ps1"))

    # --- il carico ---
    shutil.copy2(bepinex, contenuto)
    shutil.copy2(DLL, contenuto)

    trad = lwiw.assicura(os.path.join(contenuto, "traduzioni"))
    ui = 0
    for f in sorted(glob.glob(os.path.join(lwiw.TRAD, "parts", "*.json"))):
        shutil.copy2(f, trad)
        ui += 1
    dial_dir = lwiw.assicura(os.path.join(trad, "dialoghi"))
    dial = 0
    for f in sorted(glob.glob(os.path.join(lwiw.TRAD, "dialoghi", "*.json"))):
        shutil.copy2(f, dial_dir)
        dial += 1

    print(f"cartella   -> {radice}")
    print(f"  plugin      {os.path.basename(DLL)}")
    print(f"  bepinex     {os.path.basename(bepinex)}")
    print(f"  interfaccia {ui} file")
    print(f"  dialoghi    {dial} file")

    if "--no-zip" in sys.argv:
        return 0

    archivio = os.path.join(DIST, "LittleWitchInItaliano-portatile.zip")
    if os.path.isfile(archivio):
        os.remove(archivio)
    with zipfile.ZipFile(archivio, "w", zipfile.ZIP_DEFLATED) as z:
        for cartella, _, file in os.walk(radice):
            for nome in file:
                pieno = os.path.join(cartella, nome)
                z.write(pieno, os.path.join(NOME, os.path.relpath(pieno, radice)))
    mb = os.path.getsize(archivio) / (1024 * 1024)
    print(f"archivio   -> {archivio}  ({mb:.1f} MB)")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
