"""Disegna una prova del font accentato, per guardare se i segni stanno al posto giusto.

    python tools/07_prova_font.py [nome_font-IT]

Scrive Extracted/Font/prova_<nome>.png
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

from PIL import Image, ImageDraw, ImageFont  # noqa: E402

RIGHE = [
    "àèéìòù  ÀÈÉÌÒÙ",
    "Perché non l'hai già finita?",
    "Città, virtù, caffè, perciò",
    "«Sarà pronta più tardi…»",
    "ABCDEFGHIJKLM abcdefghijklm",
]


def main():
    nome = sys.argv[1] if len(sys.argv) > 1 else "JejuHallasan-IT"
    percorso = os.path.join(lwiw.EXTRACTED, "Font", nome + ".ttf")
    fnt = ImageFont.truetype(percorso, 44)

    larghezza = 900
    riga_h = 62
    img = Image.new("RGB", (larghezza, riga_h * len(RIGHE) + 30), (250, 247, 238))
    d = ImageDraw.Draw(img)
    for i, testo in enumerate(RIGHE):
        d.text((24, 14 + i * riga_h), testo, font=fnt, fill=(40, 34, 28))

    fuori = os.path.join(lwiw.EXTRACTED, "Font", f"prova_{nome}.png")
    img.save(fuori)
    print("scritto:", os.path.relpath(fuori, lwiw.ROOT))


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
