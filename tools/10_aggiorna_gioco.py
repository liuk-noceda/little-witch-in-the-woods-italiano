"""Copia plugin e traduzioni dentro il gioco, per provarli.

Non compila: fallo prima con `dotnet build -c Release`. Questo script sposta solo
gli artefatti, cosi' il giro «modifico -> provo» resta corto.

    python tools/10_aggiorna_gioco.py                 tutto, e azzera il log
    python tools/10_aggiorna_gioco.py --solo-testi    solo i JSON, log intatto

`--solo-testi` serve quando il gioco e' **aperto**: Windows tiene la DLL bloccata
finche' e' caricata, mentre i JSON si possono sovrascrivere quando si vuole —
il plugin li rilegge al prossimo avvio. Il log non viene cancellato, cosi' non si
perde l'esito dell'innesto dei dialoghi della partita in corso.
"""
import glob
import os
import shutil
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

DLL = os.path.join(lwiw.MOD, "LittleWitchIT", "bin", "Release", "netstandard2.1",
                   "LittleWitchItalian.dll")
DESTINAZIONE = os.path.join(lwiw.GIOCO, "BepInEx", "plugins", "LiukNoceda")


def main():
    solo_testi = "--solo-testi" in sys.argv

    if not solo_testi and not os.path.isfile(DLL):
        print(f"Plugin non compilato: {DLL}")
        print("Esegui prima:  dotnet build -c Release  in Mod/LittleWitchIT")
        return 1

    trad = lwiw.assicura(os.path.join(DESTINAZIONE, "traduzioni"))
    if solo_testi:
        print("plugin      -> saltato (--solo-testi)")
    else:
        try:
            shutil.copy2(DLL, DESTINAZIONE)
            print(f"plugin      -> {os.path.join(DESTINAZIONE, os.path.basename(DLL))}")
        except PermissionError:
            print("plugin      -> BLOCCATO: il gioco e' aperto. Chiudilo, oppure usa --solo-testi.")
            return 1

    n = 0
    for f in sorted(glob.glob(os.path.join(lwiw.TRAD, "parts", "*.json"))):
        shutil.copy2(f, trad)
        print(f"traduzione  -> {os.path.basename(f)}")
        n += 1
    if n == 0:
        print("nessun file in Mod/translations/parts: il plugin partira' senza traduzioni")

    dial = lwiw.assicura(os.path.join(trad, "dialoghi"))
    d = 0
    for f in sorted(glob.glob(os.path.join(lwiw.TRAD, "dialoghi", "*.json"))):
        shutil.copy2(f, dial)
        d += 1
    print(f"dialoghi    -> {d} file")

    log = os.path.join(lwiw.GIOCO, "BepInEx", "LogOutput.log")
    if not solo_testi and os.path.isfile(log):
        os.remove(log)
        print("log precedente cancellato")
    if solo_testi:
        print("\nI testi nuovi si vedranno al prossimo avvio del gioco.")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
