"""Disegna l'icona del gestore: un cappello da strega su fondo verde bosco.

Genera `Mod/manager/Resources/icon.ico` (multi-risoluzione) e i due banner PNG
che Inno Setup mostra nella procedura guidata.

    python tools/31_icona.py

Il cappello e' il simbolo piu' leggibile a 16 pixel: la sagoma resta
riconoscibile anche quando spariscono tutti i dettagli. La punta piegata e la
fibbia in glicine servono a distinguerlo da un cono qualsiasi.
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402

RES = os.path.join(lwiw.MOD, "manager", "Resources")

# La palette e' quella del gestore: verde bosco e glicine, come il villaggio.
FONDO_ALTO = (30, 42, 26)
FONDO_BASSO = (16, 22, 14)
GLICINE = (180, 140, 214)
GLICINE_SCURO = (108, 74, 143)
CREMA = (237, 232, 218)
STELLA = (245, 226, 140)


def tondo(size, raggio):
    """Maschera di un quadrato con gli angoli arrotondati."""
    m = Image.new("L", (size, size), 0)
    ImageDraw.Draw(m).rounded_rectangle([0, 0, size - 1, size - 1], raggio, fill=255)
    return m


def sfondo(size):
    """Sfumatura verticale dal verde piu' chiaro in alto a quello scuro in basso."""
    img = Image.new("RGB", (size, size))
    d = ImageDraw.Draw(img)
    for y in range(size):
        t = y / max(size - 1, 1)
        d.line([(0, y), (size, y)],
               fill=tuple(int(a + (b - a) * t) for a, b in zip(FONDO_ALTO, FONDO_BASSO)))
    return img


def cappello(d, s):
    """Il cappello, disegnato in coordinate normalizzate su un lato di `s`."""
    def p(x, y):
        return (x * s, y * s)

    # cono con la punta piegata a sinistra
    cono = [p(0.50, 0.70), p(0.365, 0.70), p(0.44, 0.36), p(0.36, 0.20),
            p(0.30, 0.15), p(0.40, 0.155), p(0.50, 0.235), p(0.545, 0.36),
            p(0.635, 0.70)]
    d.polygon(cono, fill=GLICINE_SCURO)

    # falda: un'ellisse schiacciata, con un filo di luce sopra
    d.ellipse([p(0.155, 0.615)[0], p(0, 0.615)[1],
               p(0.845, 0.795)[0], p(0, 0.795)[1]], fill=GLICINE)
    d.ellipse([p(0.175, 0.605)[0], p(0, 0.605)[1],
               p(0.825, 0.735)[0], p(0, 0.735)[1]], fill=(198, 162, 226))

    # fascia e fibbia
    d.polygon([p(0.352, 0.585), p(0.648, 0.585), p(0.663, 0.665), p(0.337, 0.665)],
              fill=CREMA)
    d.rectangle([p(0.465, 0.600)[0], p(0, 0.600)[1],
                 p(0.545, 0.652)[0], p(0, 0.652)[1]], fill=STELLA)

    # una scintilla, perche' e' pur sempre una strega
    d.polygon([p(0.735, 0.235), p(0.760, 0.300), p(0.825, 0.325),
               p(0.760, 0.350), p(0.735, 0.415), p(0.710, 0.350),
               p(0.645, 0.325), p(0.710, 0.300)], fill=STELLA)


def icona(size, super_campionamento=4):
    """Disegna a risoluzione multipla e rimpicciolisce: bordi puliti."""
    s = size * super_campionamento
    img = sfondo(s).convert("RGBA")
    img.putalpha(tondo(s, int(s * 0.22)))
    d = ImageDraw.Draw(img)
    cappello(d, s)
    return img.resize((size, size), Image.LANCZOS)


def banner(larghezza, altezza, con_cappello=True):
    """Immagine della procedura guidata: sfumatura, cappello e un po' di grana."""
    img = Image.new("RGB", (larghezza, altezza))
    d = ImageDraw.Draw(img)
    for y in range(altezza):
        t = y / max(altezza - 1, 1)
        d.line([(0, y), (larghezza, y)],
               fill=tuple(int(a + (b - a) * t) for a, b in zip(FONDO_ALTO, FONDO_BASSO)))

    # aloni tenui, cosi' il fondo non sembra una tinta piatta
    alone = Image.new("RGB", (larghezza, altezza), (0, 0, 0))
    da = ImageDraw.Draw(alone)
    da.ellipse([-altezza // 2, altezza // 3, larghezza // 2, altezza * 2],
               fill=(40, 30, 55))
    alone = alone.filter(ImageFilter.GaussianBlur(altezza // 4))
    img = Image.blend(img, Image.blend(img, alone, 0.5), 0.6)

    if con_cappello:
        lato = int(altezza * 0.72)
        c = icona(lato)
        img.paste(c, ((larghezza - lato) // 2, (altezza - lato) // 2), c)
    return img


def main():
    lwiw.assicura(RES)

    misure = [256, 128, 64, 48, 32, 24, 16]
    immagini = [icona(m) for m in misure]
    percorso = os.path.join(RES, "icon.ico")
    immagini[0].save(percorso, format="ICO",
                     sizes=[(m, m) for m in misure],
                     append_images=immagini[1:])
    print(f"icona      -> {percorso}  ({', '.join(str(m) for m in misure)})")

    png = os.path.join(RES, "logo.png")
    icona(256).save(png)
    print(f"logo       -> {png}")

    # misure fissate da Inno Setup
    b = os.path.join(RES, "banner.png")
    banner(164, 314).save(b)
    print(f"banner     -> {b}  (164x314)")

    bp = os.path.join(RES, "banner_small.png")
    banner(55, 58).save(bp)
    print(f"banner sm  -> {bp}  (55x58)")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
