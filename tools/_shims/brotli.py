"""Tappo per UnityPy su ARM64.

`brotli` non ha wheel per win-arm64 e non compila qui, ma UnityPy lo importa a
prescindere in `helpers/CompressionHelper.py`. I bundle di Little Witch usano LZ4,
non brotli: il modulo non viene mai chiamato davvero. Se un giorno servisse, questo
tappo lo dice invece di restituire dati sbagliati.
"""


def decompress(*_args, **_kwargs):
    raise NotImplementedError(
        "brotli non e' installato (nessuna wheel per win-arm64). "
        "Un bundle compresso con brotli e' arrivato fin qui: installare brotli davvero."
    )


def compress(*_args, **_kwargs):
    raise NotImplementedError("brotli non e' installato (nessuna wheel per win-arm64).")
