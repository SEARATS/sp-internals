# Changelog

Merges add notes under `## Unreleased`; a release turns it into `## x.y.z - <date>` ([release process](docs/release-process.md)).

## Unreleased

Nothing has been released yet; `SpInternals.csproj` says 0.2.0 until the first release day picks the number.

- Generates named C# bindings from the installed Sea Power assemblies (`Seapower-Scripts`, `Noesis.NoesisGUI`, `Assets`, `Libraries`, `Plugins`)
- `SpInternals.targets`: embed the bindings in a mod's own DLL instead of shipping `SP.Internals.dll`
