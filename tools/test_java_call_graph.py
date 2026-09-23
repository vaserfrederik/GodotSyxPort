"""Check inherited file edges and avoid guessing between competing interfaces."""
import hashlib
import json
from pathlib import Path
import tempfile
from zipfile import ZipFile

import build_java_call_graph as graph


sources = {
    "parity/Base.java": "package parity; class Base { void ping() {} void inherited() {} }",
    "parity/Derived.java": "package parity; class Derived extends Base { void run() { ping(); } void ping() { super.ping(); } }",
    "parity/Face.java": "package parity; interface Face { void draw(); }",
    "parity/Abstract.java": "package parity; abstract class Abstract implements Face {}",
    "parity/Left.java": "package parity; interface Left { void clash(); }",
    "parity/Right.java": "package parity; interface Right { void clash(); }",
    "parity/Both.java": "package parity; abstract class Both implements Left, Right {}",
    "parity/Caller.java": """package parity;
        class Caller {
            void run(Derived derived, Abstract abstractValue, Both both) {
                derived.ping(); derived.run(); derived.inherited();
                abstractValue.draw(); both.clash();
            }
        }
    """,
}

with tempfile.TemporaryDirectory() as directory:
    root = Path(directory)
    jar_path = root / "sources.jar"
    with ZipFile(jar_path, "w") as jar:
        for path, source in sources.items():
            jar.writestr(path, source)
    registry = {
        "source_archive_sha256": hashlib.sha256(jar_path.read_bytes()).hexdigest(),
        "entries": [{"JavaSource": path, "JavaTypes": [
            ("INTERFACE " if "interface " in source and "class " not in source else "CLASS ")
            + Path(path).stem], "JavaDependencies": [], "CSharpImplementation": []}
            for path, source in sources.items()],
    }
    registry_path = root / "registry.json"
    registry_path.write_text(json.dumps(registry))
    graph.REGISTRY = registry_path
    result = graph.build(jar_path)
    graph.validate(result)
    edges = {(row["CallerJavaSource"], row["TargetJavaSource"]): row["CandidateCalls"]
             for row in result["edges"]}
    assert edges == {
        ("parity/Derived.java", "parity/Base.java"): 1,
        ("parity/Caller.java", "parity/Derived.java"): 2,
        ("parity/Caller.java", "parity/Base.java"): 1,
        ("parity/Caller.java", "parity/Face.java"): 1,
    }, edges
    assert result["summary"]["unresolved_calls_by_reason"]["inherited-ambiguous"] == 1

print("Java inheritance: base class and interface calls resolved; competing interfaces deferred")
