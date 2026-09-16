# Seasonal SLS Bridge

**Seasonal SLS Bridge** is a Valheim compatibility bridge that connects **Seasonality** with **StarLevelSystem (SLS)**.

The mod gives creatures a seasonal identity by assigning a single weighted StarLevelSystem modifier according to the current season.

> **The world should feel different as the seasons change without replacing the systems that already make Valheim work.**

## Features

### Winter — `Invernal`

Possible modifiers:

- `Frost`
- `ResistFrost`
- `Big`
- `StaminaDrain`
- `ResistPierce`
- `ResistSlash`
- `ResistBlunt`
- `EitrDrain`

### Spring — `Primaveral`

Possible modifiers:

- `Poison`
- `ElementalChaos`
- `PoisonNova`
- `ResistPoison`
- `Fast`
- `Evolving`
- `Splitter`

### Summer — `Estival`

Possible modifiers:

- `Fire`
- `FireNova`
- `ResistFire`
- `Lightning`
- `Fast`
- `Brutal`
- `Big`

### Autumn — `Otoñal`

Possible modifiers:

- `Lightning`
- `Poison`
- `ElementalChaos`
- `SoulEater`
- `ResistSpirit`
- `Alert`
- `ResistPoison`
- `Big`

## How it works

When a creature is spawned, the bridge waits for a configurable delay before performing the seasonal roll.

```text
Creature spawned
       │
       ▼
Character.Awake
       │
       ▼
Configurable delay
       │
       ▼
Read current season
       │
       ▼
Roll seasonal modifier
       │
       ▼
Call StarLevelSystem
       │
       ▼
Modifier successfully applied
       │
       ▼
Register seasonal visual title
```

Only **one seasonal modifier** is selected per creature.

The bridge does not directly manipulate creature statistics. It asks StarLevelSystem to apply one of its existing modifiers.

## Seasonal creature titles

Seasonal names are visual information only and do not replace the StarLevelSystem modifier name.

Example:

```text
Invernal Esqueleto [Poison]
```

- `Invernal` → seasonal title added by Seasonal SLS Bridge
- `Esqueleto` → Valheim creature name
- `[Poison]` → StarLevelSystem modifier

The bridge deliberately keeps the original SLS modifier untouched.

## Configuration

The plugin generates its configuration through BepInEx.

| Setting | Default | Description |
|---|---:|---|
| `SeasonalRollChance` | `100` | Percentage chance of attempting a seasonal modifier |
| `DelaySeconds` | `0.75` | Delay before the seasonal roll |
| `IncludeTamed` | `false` | Allows tamed creatures to receive seasonal modifiers |
| `LogAppliedModifiers` | `true` | Logs successful modifier applications |
| `LogApiDiscovery` | `true` | Logs discovered StarLevelSystem API methods |

## Compatibility approach

Seasonal SLS Bridge intentionally avoids modifying StarLevelSystem's internal implementation.

Instead, it discovers compatible StarLevelSystem modifier methods and calls the existing modifier system.

The bridge currently looks for methods named:

```text
AddModifierToCreature
AddModifierToTargetCreature
```

The reflection layer checks the method signature before attempting to use it.

## What the bridge does NOT do

- Replace StarLevelSystem.
- Replace Seasonality.
- Rewrite SLS modifier names.
- Directly edit creature statistics.
- Modify StarLevelSystem's modifier definitions.
- Create a second creature-level progression system.
- Modify player characters.
- Require manual modification of third-party mod files.

## Requirements

The project is intended to run alongside:

- Valheim
- BepInEx
- Jötunn
- StarLevelSystem
- Seasonality

Exact dependency versions should be checked against the release being compiled.

## Source code

This repository contains the **uncompiled source code** of Seasonal SLS Bridge.

The source is public so that Valheim modders and C# developers can inspect, review and improve the implementation.

## AI-assisted development

Seasonal SLS Bridge has been developed with significant assistance from artificial intelligence.

The author is not a professional C# or Valheim mod developer.

AI assistance has been used for tasks including:

- Writing and restructuring C# code.
- Understanding BepInEx and Harmony.
- Understanding interactions between Valheim mods.
- Working with the StarLevelSystem API.
- Debugging compiler and runtime errors.
- Investigating compatibility problems.
- Designing safer integration approaches.
- Reviewing and improving existing code.
- Preparing documentation and release packages.

This disclosure is intentional. Experienced Valheim modders and C# developers are encouraged to review the source and report bugs, compatibility issues, performance concerns or safer implementations.

## Development philosophy

The main goal of the project is **immersion and compatibility**.

The project generally prefers:

1. Existing systems over duplicated systems.
2. Small compatibility layers over invasive modifications.
3. Stable behaviour over unnecessary complexity.
4. Configurability over hard-coded gameplay decisions.
5. Clear logging when diagnosing compatibility problems.
6. Conservative changes when working with third-party APIs.

## Building the project

The repository contains the source project required to compile the plugin.

References to the appropriate Valheim/BepInEx/Jötunn/StarLevelSystem assemblies should point to the developer's own local modding environment.

Third-party assemblies are intentionally not included in this repository.

Do not commit generated build directories such as:

```text
bin/
obj/
```

## Contributing

Please read [`CONTRIBUTING.md`](CONTRIBUTING.md) before submitting code changes or larger improvements.

Bug fixes, compatibility improvements, documentation corrections and code reviews are welcome.

## Credits

**Seasonal SLS Bridge**

Created by **TheLionCid**.

The project interacts with systems created by other developers, including:

- BepInEx
- Jötunn
- StarLevelSystem
- Seasonality
- Valheim

Please respect the individual licenses and terms of the projects on which this mod depends.

## License

See [`LICENSE`](LICENSE) for the license of the Seasonal SLS Bridge source code.

Dependencies remain the property of their respective authors and are subject to their own licenses.

## Disclaimer

Seasonal SLS Bridge is an independent compatibility project.

It is not an official component of Valheim, StarLevelSystem, Seasonality, Jötunn or BepInEx.

Updates to Valheim or any dependency may require changes to the bridge.
