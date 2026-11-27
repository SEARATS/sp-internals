# Sea Power internals

Named C# bindings for managed types and members in Sea Power. The build reads
metadata from your local installation and generates a .NET Standard 2.1 library
for mods. The default inputs are `Seapower-Scripts`, `Noesis.NoesisGUI`,
`Assets`, `Libraries` and `Plugins`.

## Build

Install the .NET 10 SDK and run this from the checkout.

```powershell
dotnet build src/SpInternals -c Release
```

The build uses the default Steam location. Set `SEA_POWER_DIR` or pass
`-p:SeaPowerDir=<path>` for another installation. Use
`-p:InternalsExtraAssemblies=UnityEngine.CoreModule` to add an assembly
from the same Managed directory, or `-p:InternalsAssemblies=Noesis.NoesisGUI`
to replace the default list. The generator also accepts DLL paths directly.

```powershell
dotnet run --project tools/Generate -- path/to/Bindings.g.cs path/to/Example.dll
```

Reference `src/SpInternals/SpInternals.csproj` from a mod project and ship
`SP.Internals.dll` alongside the mod. The library runs inside the game's
existing Mono process. It has no Anchor Chain or Harmony dependency.

## Embed in your mod DLL

To compile the generated bindings and access helpers into your own DLL,
import `SpInternals.targets` in your mod's `.csproj`. Adjust the relative path
to your checkout or submodule. The mod project needs C# 12 or later.

```xml
<Import Project="../../external/sp-internals/src/SpInternals/SpInternals.targets" />
```

Use this import in place of the `SP.Internals` project reference. The embedded
types are internal to your mod, and shipping the mod DLL includes them.
The assembly selection and local game path settings work in either mode.

## Use in a mod

```csharp
using SeaPower;
using SP.Internals;
using Game = SP.Internals.Generated.Seapower_Scripts;
using Gui = SP.Internals.Generated.Noesis_NoesisGUI;

// Cache the compiled getter once, then call it with a MissionManager.
static readonly Func<MissionManager, int> ReadNextPatrolZoneNumber =
    Game.SeaPower_MissionManager.Fields._nextPatrolZoneNumber.Info.Getter<MissionManager, int>();

// A MethodInfo can also be passed directly to Harmony.
static readonly System.Reflection.MethodInfo SetBindingFromPath =
    Gui.Noesis_FrameworkElement.Methods.SetBinding__DependencyProperty_String.Info;
```

Each generated type has a `Type.Info` and groups for its declared fields,
methods, constructors, properties and events. The groups cover private
members, nested types and compiler-generated definitions. Build once in
your editor's active configuration to enable completion. IntelliSense
shows the original signatures, which you can also browse in
`obj/<Configuration>/<TargetFramework>/Bindings.g.cs` under the project
that generates them. Overloads include parameter type names in their
identifiers. A numeric suffix disambiguates names that still collide.

Use `Bind<TDelegate>()` to cache a method delegate or
`Factory<TDelegate>()` for a constructor. Property and event accessors
are method bindings too. Use `StaticGetter` and `StaticSetter` for static
fields, and `RefSetterFor` to mutate a struct field by reference. Readonly
fields and constants have read access. For members declared on a generic
type, use `On(typeof(YourType<int>))` for the constructed type. Close a
generic method with `MakeGenericMethod` before binding its delegate.

Bindings resolve by metadata token and check the loaded module's version ID
before use. Rebuild after a game update and check any changed signatures.
Generated files stay local, and the project does not copy game DLLs or
launch the game. Coverage is for managed definitions in the selected DLLs.
Native code behind Unity and Noesis calls has no managed member bindings.
