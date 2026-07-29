# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-07-29

### Changed

- **BREAKING** `Formation` and `IFormationMember` are now generic in the member type:
  `Formation<T>` and `IFormationMember<T> where T : IFormationMember<T>`. Members no longer need to be
  cast back from an interface, and all four concrete formations (`Trail<T>`, `Echelon<T>`,
  `ArrowHead<T>`, `BattleSpread<T>`) take the same parameter. See the upgrade table in the README.
- **BREAKING** `IFormationMember` requires a new `T Self { get; }` member, implemented as `=> this`.
- **BREAKING** Removed `velocity`, `angularVelocity` and `Transform` from `IFormationMember`. The core no
  longer touches `Transform`, so movement and physics stay entirely in consumer code.
- **BREAKING** The runtime assembly was renamed from `com.artisangames.formationsystem` to
  `FormationSystem`. Update the `references` array of any dependent `.asmdef`.
- **BREAKING** The minimum supported editor version is now Unity 2021.3. The previous `2019.1` floor was
  incorrect — the public interfaces use C# 8 syntax, which Unity only supports from 2020.2 onward.
- `Formation.leader` now has a `protected` setter instead of `private`, so subclasses can manage it.
- The test assembly was renamed from `com.artisangames.formationsystem.tests` to `FormationSystem.Tests`.

### Fixed

- `AddMember` now sets the member's `Formation` back-reference, which was previously left `null`.
- `AddMember` no longer corrupts `PositionIndex` when handed a member the formation already contains.
  `members` is a `HashSet<T>`, so the add was a no-op, but the following lines still reassigned the
  member's index to `MemberCount - 1` — a slot another member already owned. Adding A, then B, then A
  again left both A and B claiming index 1 while `leader` still pointed at A, so the next
  `RemoveMember` reshuffled off corrupt indices. The two existing duplicate-add tests only re-added a
  lone member, where `MemberCount - 1` happened to be the index it already had, so they never caught
  it; `Re_Adding_A_Member_Does_Not_Steal_Another_Members_Position_Index` now covers it for all four
  formations.
- Removing the last member of a formation now clears `leader` to `default(T)`. Previously it kept
  pointing at the removed member, and `BalancedFormation.RemoveMember` could dereference a `null` flank
  member while rebalancing an empty formation.

### Removed

- The `net.tnrd.nsubstitute` dependency. No test in the package used NSubstitute, so every consumer was
  being forced to install a mocking library from a third-party git URL. Tests use a hand-written
  `TestFormationMember` stub instead.
- `FormationTestsUtility`, whose index assertion helper is now a `protected static` member of the
  `FormationTests` base class.

### Added

- `testables` entry in `package.json` and instructions for exposing the package's edit-mode tests in the
  Test Runner.
- README with installation instructions, per-formation offset tables, API reference and a 1.x upgrade
  guide.
- MIT `LICENSE.md`. The package previously had no license, which left it all-rights-reserved.
- `license`, `repository`, `documentationUrl`, `changelogUrl` and `licensesUrl` metadata in
  `package.json`.

## [1.0.2] - 2025-12-28

### Changed

- Pointed the `net.tnrd.nsubstitute` dependency at its GitHub URL.

## [1.0.1] - 2025-12-28

### Added

- `testables` field in `package.json`.

## [1.0.0] - 2025-12-28

### Added

- Initial release: `Formation` base class with `Trail`, `Echelon`, `ArrowHead` and `BattleSpread`
  layouts, plus `BalancedFormation` for symmetrical shapes with rebalancing removal.

> **Note on the 1.x history.** No release was ever tagged, and a later bulk file upload reset
> `package.json` back to `1.0.0` and dropped the `testables` field. The 1.0.1 and 1.0.2 entries above are
> reconstructed from the commit history; the manifest on `main` read `1.0.0` right up until 2.0.0. The
> `compare` links below only resolve once the corresponding tags exist.

[2.0.0]: https://github.com/NoumanWaheed93/FormationFramework/compare/v1.0.2...v2.0.0
[1.0.2]: https://github.com/NoumanWaheed93/FormationFramework/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/NoumanWaheed93/FormationFramework/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/NoumanWaheed93/FormationFramework/releases/tag/v1.0.0
