"""Check explicit Java receiver hints against scoped source declarations."""
import json
from pathlib import Path
import subprocess
import tempfile
from zipfile import ZipFile


source = """package parity;
class Target { void ping() {} }
class Caller {
    private Target field;
    void call(Target parameter) { field.ping(); parameter.ping(); }
    void shadow() { Target field = null; field.ping(); }
}
"""

with tempfile.TemporaryDirectory() as directory:
    archive = Path(directory) / "sources.jar"
    with ZipFile(archive, "w") as jar:
        jar.writestr("parity/Caller.java", source)
    process = subprocess.run(
        ["java", str(Path(__file__).with_name("JavaCallInventory.java")), str(archive)],
        text=True, capture_output=True, check=True,
    )
    record = json.loads(process.stdout.strip())
    hints = {(call["caller"], call["receiver"]): call["receiverTypeHint"]
             for call in record["Calls"] if call["name"] == "ping"}
    assert hints == {
        ("Caller#call/1", "field"): "Target",
        ("Caller#call/1", "parameter"): "Target",
        ("Caller#shadow/0", "field"): "",
    }, hints

print("Java AST receiver hints: field, parameter and local shadowing passed")
