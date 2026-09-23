"""Check explicit Java receiver hints against scoped source declarations."""
import json
from collections import defaultdict
from pathlib import Path
import subprocess
import tempfile
from zipfile import ZipFile


source = """package parity;
class Target { void ping() {} }
class Other { void ping() {} }
class Caller {
    private Target field;
    void call(Target parameter) { field.ping(); parameter.ping(); }
    void local() { Target local = null; local.ping(); }
    void shadow() { Other field = null; field.ping(); }
    void inferred() { var local = new Target(); local.ping(); }
    void loop() { for (Target item = null; true;) { item.ping(); break; } }
    void siblings() {
        { Other value = null; value.ping(); }
        { Target value = null; value.ping(); }
    }
    void lambda() {
        java.util.function.Consumer<Target> action = (Target item) -> item.ping();
    }
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
    hints = defaultdict(set)
    for call in record["Calls"]:
        if call["name"] == "ping":
            hints[(call["caller"], call["receiver"])].add(call["receiverTypeHint"])
    assert hints == {
        ("Caller#call/1", "field"): {"Target"},
        ("Caller#call/1", "parameter"): {"Target"},
        ("Caller#local/0", "local"): {"Target"},
        ("Caller#shadow/0", "field"): {"Other"},
        ("Caller#inferred/0", "local"): {""},
        ("Caller#loop/0", "item"): {"Target"},
        ("Caller#siblings/0", "value"): {"Other", "Target"},
        ("Caller#lambda/0", "item"): {"Target"},
    }, hints

print("Java AST receiver hints: field, parameter, local, loop and lambda scopes passed")
