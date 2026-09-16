# Contributing to Seasonal SLS Bridge

Thank you for your interest in Seasonal SLS Bridge.

This project is intentionally open to code review and contributions, especially from people with experience in:

- C#
- Unity
- Valheim modding
- BepInEx
- Harmony
- Jötunn
- StarLevelSystem
- Seasonality

The author is not a professional Valheim or C# developer, and the project has been developed with significant AI assistance. Technical review is therefore particularly valuable.

## What you can contribute

Useful contributions include:

- Bug fixes.
- Compatibility fixes.
- Performance improvements.
- Reflection improvements.
- Harmony patch improvements.
- API compatibility improvements.
- Cleaner C# implementations.
- Documentation corrections.
- Configuration improvements.
- Better error handling.
- Testing against different versions of dependencies.

A small, well-tested improvement is welcome.

## Code review

Code reviews are especially encouraged.

If you notice something that could be safer, faster, simpler, more compatible, more idiomatic C#, less dependent on reflection, less invasive to Valheim, or easier to maintain, please explain the reasoning behind the suggestion.

## Before opening an issue

Please check whether the problem has already been reported.

When reporting a problem, include as much relevant information as possible:

```text
Valheim version:
BepInEx version:
Seasonal SLS Bridge version:
StarLevelSystem version:
Seasonality version:
Jötunn version:
Other relevant mods:
```

Also include:

- What happened.
- What you expected to happen.
- Whether the problem happens consistently.
- Relevant BepInEx log output.
- Relevant configuration settings.
- Steps required to reproduce the problem.

Remove personal information from logs before posting them.

## Bug reports

A useful bug report should answer:

### What happened?

Describe the actual behaviour.

### What should have happened?

Describe the expected behaviour.

### How can it be reproduced?

Provide the smallest set of steps possible.

### Does disabling another mod fix it?

If you have tested this, mention the result.

This can be particularly important for compatibility problems.

## Pull requests

For code changes:

1. Fork the repository.
2. Create a branch for your change.
3. Make the smallest reasonable change.
4. Build the project.
5. Test the change in Valheim when possible.
6. Document important behaviour changes.
7. Submit a pull request.

Example branch names:

```text
fix/sls-api-detection
fix/seasonal-title
improvement/reflection
docs/configuration
```

## Keep changes focused

Please avoid combining unrelated changes in the same pull request.

For example, a pull request containing a compatibility fix, a complete formatting change and an unrelated README rewrite is harder to review than several focused changes.

Prefer small, clearly defined changes.

## Compatibility first

Seasonal SLS Bridge is a compatibility mod.

When choosing between two implementations, prefer the approach that introduces fewer assumptions about third-party mods.

Be particularly careful when changing:

- Reflection logic.
- Harmony patches.
- SLS API calls.
- Creature lifecycle handling.
- Spawn timing.
- Modifier application.
- Third-party assembly discovery.

A change that works with one version but breaks another should be treated carefully.

## Third-party code

Do not copy code from another mod into this repository unless its license explicitly permits it.

If an implementation is inspired by another project, identify the project and check its license before using code from it.

Dependencies should remain dependencies whenever possible.

## AI-assisted development

AI tools have been used significantly during the development of this project.

AI-generated or AI-assisted code should still be treated like any other code: **it needs human review and testing.**

If you find code that is unnecessarily complicated, incorrect, unsafe, inefficient, incompatible with Unity, incompatible with BepInEx, incompatible with a dependency, or simply a poor implementation, please point it out.

A technically justified correction is welcome regardless of whether the original code was written manually or with AI assistance.

## Testing

Before submitting a code change, test it when possible with the intended Valheim environment.

At minimum, check:

- The plugin loads without exceptions.
- Seasonal modifiers are still applied.
- Creature names remain intact.
- Seasonal titles do not duplicate.
- Players are not affected.
- Tamed creatures follow the configured setting.
- StarLevelSystem remains functional.
- Seasonality remains functional.
- No repeated Harmony errors appear in the log.

If the change affects compatibility with a particular mod version, mention the tested version in the pull request.

## Discussion

If you are unsure whether an idea is appropriate for the project, open an issue first.

For larger architectural changes, discussing the approach before writing a large pull request can save everyone time.

## Respect the dependencies

Seasonal SLS Bridge relies on other projects.

Please do not report normal behaviour of Valheim, StarLevelSystem, Seasonality, BepInEx or Jötunn as bugs in this project without first determining whether the bridge is actually involved.

When possible, test whether the problem persists with Seasonal SLS Bridge disabled.

## Final principle

The purpose of this repository is not to claim that the code is perfect.

It is to make the code **visible, reviewable and improvable**.

If you can make Seasonal SLS Bridge safer, more compatible, easier to understand or easier to maintain, your contribution is welcome.
