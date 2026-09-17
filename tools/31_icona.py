"""Generatore icona e grafica installer per Little Witch in the Woods.

Delega a `tools/32_export_installer_art.py`, che estrae dagli asset originali
del gioco l'illustrazione ufficiale di Ellie e Virgil per generare logo, icona
multi-risoluzione e banner Inno Setup.

    python tools/31_icona.py
"""
import os
import subprocess
import sys

TOOLS = os.path.dirname(os.path.abspath(__file__))
SCRIPT_32 = os.path.join(TOOLS, "32_export_installer_art.py")


def main():
    return subprocess.call([sys.executable, SCRIPT_32])


if __name__ == "__main__":
    sys.exit(main())

