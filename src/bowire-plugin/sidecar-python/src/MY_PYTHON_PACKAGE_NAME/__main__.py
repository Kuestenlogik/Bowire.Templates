"""Entry point — ``python -m MY_PYTHON_PACKAGE_NAME`` boots the plugin
over stdio.

The Bowire host spawns this as a subprocess (see ``sidecar.json``);
we hand the plugin instance to :func:`bowire_plugin.run`, which
speaks JSON-RPC 2.0 over NDJSON on stdin/stdout. For HTTP/SSE-mode
sidecars swap ``run`` for ``run_http(plugin, host=..., port=...)`` and
flip ``transport`` to ``"http"`` in ``sidecar.json``.
"""

from bowire_plugin import run

from .plugin import MyProtocol


if __name__ == "__main__":
    raise SystemExit(run(MyProtocol()))
