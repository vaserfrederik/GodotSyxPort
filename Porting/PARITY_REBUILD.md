# Parity rebuild

Baseline: A108 (`582e408`). The `parity-rebuild` branch keeps the existing game
while replacing unsupported equivalence claims with repeatable checks.

Input archives supplied for this rebuild (SHA-256):

| Archive | Files | SHA-256 |
|---|---:|---|
| Java sources JAR | 2,443 `.java` | `a3fc2aa9f4be31aec0616a5030a656ac49a6011343b7e91395c7bac0b9dfb0a5` |
| Runtime JAR | 11,876 entries | `fe0137c05408e5ee9e31f06fc44fe37a5d4372d9c985b280f0852d1347c0224b` |
| Data ZIP | 1,537 entries | `d9e251707e881c83567782d2f659bfc79ca959d9a861cde83435d5459cc51403` |

## Source inventory

`parity_registry.json` is generated from the user-supplied Java source JAR via
the JDK compiler's syntax tree API. The Java source and supplied game assets are
not copied into this repository. The file records their archive hash and Java
type declarations, explicit imports, candidate C# implementations, and the
evidence required before a parity claim can be made.

The 900 candidate implementations from `legacy_compile_manifest.json` start as
`PortedUntested`. No item currently claims `ParityPassed` or
`EngineReplacementPassed`. The other 1,543 source units start as `NotStarted`.
The older manifest contains 2,465 C# files; that count is not the number of
Java units in this specific source JAR.

Recreate the registry from a locally supplied archive:

```sh
python tools/build_parity_registry.py --source-jar /path/to/SongsOfSyx-sources.jar
python tools/build_parity_registry.py --check --source-jar /path/to/SongsOfSyx-sources.jar
```

CI checks the committed registry without requiring a copyrighted source archive.
It rejects missing implementation files and claims of parity without named tests.
Reproduction against the source archive requires the second command locally.

## Scope and next gate

`JavaDependencies` currently contains only explicit imports from the Java AST.
It does not yet include same-package references, resolved method calls, runtime
dependencies, or Java-to-C# execution parity. Those require a reference runner
and dependency resolution before any status can advance to `ParityPassed`.

The C# build is checked in CI. The current workspace has no .NET SDK, so its
first CI run is also the initial compilation check for this branch.
