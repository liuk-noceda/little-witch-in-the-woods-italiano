# -*- coding: utf-8 -*-
"""
Estrae dagli asset originali di «Little Witch in the Woods» l'illustrazione
ufficiale di Ellie con Virgil e prepara le risorse grafiche per il gestore WPF
e l'installer Inno Setup (ritagliate, rifinite, con trasparenza e multi-risoluzione).

Come in Rubinite (47_export_installer_art.py), si usa l'arte originale del gioco
invece di forme geometriche stilizzate.

Vincolo tecnico noto: UnityPy non decodifica le texture compresse su Windows ARM64
perché la DLL nativa astc_encoder è compilata solo per x64. L'illustrazione principale
(Ending_Illustration) è fortunatamente in formato RGBA32 non compresso (formato 4),
quindi i byte grezzi vengono montati direttamente con PIL/Pillow senza dipendere
da librerie native incompatibili.

    python tools/32_export_installer_art.py
"""
import glob
import os
import sys
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lwiw  # noqa: E402
import UnityPy  # noqa: E402

DATA = lwiw.DATA
RES = os.path.join(lwiw.MOD, "manager", "Resources")
RAW = os.path.join(lwiw.EXTRACTED, "installer_art")

MODES = {3: ("RGB", 3), 4: ("RGBA", 4), 5: ("ARGB", 4), 14: ("BGRA", 4)}


def load_raw_texture(name, asset_name="sharedassets168.assets"):
    percorso = os.path.join(DATA, asset_name)
    if not os.path.exists(percorso):
        candidati = glob.glob(os.path.join(DATA, "sharedassets*.assets"))
    else:
        candidati = [percorso]

    for fpath in candidati:
        try:
            env = UnityPy.load(fpath)
            for o in env.objects:
                if o.type.name != "Texture2D":
                    continue
                if (o.peek_name() or "") != name:
                    continue
                t = o.read_typetree()
                fmt = t.get("m_TextureFormat")
                if fmt not in MODES:
                    print(f"[!] {name} in {os.path.basename(fpath)}: formato {fmt} compresso")
                    continue
                mode, bpp = MODES[fmt]
                w, h = t["m_Width"], t["m_Height"]

                data = bytes(t.get("image data") or b"")
                sd = t.get("m_StreamData") or {}
                if not data and sd.get("path"):
                    res_path = os.path.join(DATA, os.path.basename(sd["path"]))
                    with open(res_path, "rb") as f:
                        f.seek(int(sd["offset"]))
                        data = f.read(int(sd["size"]))

                need = w * h * bpp
                if len(data) < need:
                    print(f"[!] {name}: byte insufficienti ({len(data)} < {need})")
                    continue

                img = Image.frombytes(mode, (w, h), data[:need])
                if mode == "ARGB":
                    a, r, g, b = img.split()
                    img = Image.merge("RGBA", (r, g, b, a))
                elif mode == "BGRA":
                    b, g, r, a = img.split()
                    img = Image.merge("RGBA", (r, g, b, a))
                elif mode == "RGB":
                    img = img.convert("RGBA")

                return img.transpose(Image.FLIP_TOP_BOTTOM)
        except Exception:
            continue
    return None


def crea_squircle(img, raggio_ratio=0.22):
    s = img.width
    maschera = Image.new("L", (s, s), 0)
    d = ImageDraw.Draw(maschera)
    d.rounded_rectangle([0, 0, s - 1, s - 1], radius=int(s * raggio_ratio), fill=255)

    ris = img.copy()
    ris.putalpha(maschera)
    return ris


def main():
    lwiw.assicura(RES)
    lwiw.assicura(RAW)

    print("[*] Ricerca dell'artwork ufficiale di Ellie negli asset di gioco...")
    raw_path = os.path.join(RAW, "Ending_Illustration.png")
    if os.path.exists(raw_path):
        print(f"    Uso cache locale estratta: {raw_path}")
        art = Image.open(raw_path).convert("RGBA")
    else:
        art = load_raw_texture("Ending_Illustration", "sharedassets168.assets")
        if art is None:
            sys.exit("[!] Impossibile estrarre Ending_Illustration dagli asset di gioco.")
        art.save(raw_path)
        print(f"    Estratta {art.width}x{art.height} RGBA in {raw_path}")

    box_ellie = (775, 20, 1287, 532)
    ellie_square = art.crop(box_ellie)

    ellie_icon_src = crea_squircle(ellie_square, 0.22)

    logo_path = os.path.join(RES, "logo.png")
    logo = ellie_icon_src.resize((256, 256), Image.LANCZOS)
    logo.save(logo_path)
    print(f"[OK] logo.png              256x256 -> {logo_path}")

    ico_path = os.path.join(RES, "icon.ico")
    misure = [256, 128, 64, 48, 32, 24, 16]
    immagini_ico = [ellie_icon_src.resize((m, m), Image.LANCZOS) for m in misure]
    immagini_ico[0].save(
        ico_path,
        format="ICO",
        sizes=[(m, m) for m in misure],
        append_images=immagini_ico[1:],
    )
    print(f"[OK] icon.ico              multi-risoluzione ({', '.join(str(m) for m in misure)}) -> {ico_path}")

    banner_path = os.path.join(RES, "banner.png")
    crop_h = art.height
    crop_w = int(crop_h * 164 / 314)
    x1 = 740
    banner_crop = art.crop((x1, 0, x1 + crop_w, crop_h))
    banner_img = banner_crop.resize((164, 314), Image.LANCZOS).convert("RGB")
    banner_img.save(banner_path)
    print(f"[OK] banner.png            164x314 -> {banner_path}")

    banner_sm_path = os.path.join(RES, "banner_small.png")
    sq_small = ellie_icon_src.resize((50, 50), Image.LANCZOS)
    banner_sm = Image.new("RGBA", (55, 58), (0, 0, 0, 0))
    banner_sm.paste(sq_small, ((55 - 50) // 2, (58 - 50) // 2), sq_small)
    banner_sm.save(banner_sm_path)
    print(f"[OK] banner_small.png      55x58 -> {banner_sm_path}")

    print(f"\n[*] Risorse generate con successo in: {RES}")
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.exit(main())
