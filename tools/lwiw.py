"""Percorsi e bootstrap comuni a tutti gli script del progetto."""
import os
import sys

TOOLS = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(TOOLS)

# Il tappo brotli deve entrare nel path prima che UnityPy venga importato.
sys.path.insert(0, os.path.join(TOOLS, "_shims"))

GIOCO = os.environ.get("LWIW_GIOCO", r"C:\Games\Little Witch in the Woods")
DATA = os.path.join(GIOCO, "LWIW_Data")
MANAGED = os.path.join(DATA, "Managed")
AA = os.path.join(DATA, "StreamingAssets", "aa", "StandaloneWindows64")

EXTRACTED = os.path.join(ROOT, "Extracted")
MOD = os.path.join(ROOT, "Mod")
TRAD = os.path.join(MOD, "translations")


def bundle(nome):
    """Percorso completo di un bundle addressable."""
    return os.path.join(AA, nome)


def assicura(cartella):
    os.makedirs(cartella, exist_ok=True)
    return cartella
