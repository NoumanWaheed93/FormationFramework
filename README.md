# Formation System

[![openupm](https://img.shields.io/badge/upm-2.0.0-blue)](https://github.com/NoumanWaheed93/FormationFramework)
[![license](https://img.shields.io/badge/license-MIT-green)](LICENSE.md)
[![unity](https://img.shields.io/badge/unity-2021.3%2B-black)](https://unity.com)

Index-based formation positioning for squads, wings and groups in Unity.

Each member of a formation owns a **position index** (`0` is the leader). The formation turns that index
into a relative offset from the leader. When a member is removed, the remaining indices are reshuffled so
the shape stays intact and no gaps are left behind — including when the leader itself is removed.

The core is plain C#: no `MonoBehaviour`, no scene dependency, no singletons. It computes offsets and
nothing else, so moving, steering and interpolating your units stays entirely your code's job — and the
formation logic stays unit testable.

## Contents

- [Install](#install)
- [Quick start](#quick-start)
- [Formations](#formations)
- [API](#api)
- [Spacing](#spacing)
- [Running the tests](#running-the-tests)
- [Upgrading from 1.x](#upgrading-from-1x)
- [License](#license)

## Install

Add the package via **Window ▸ Package Manager ▸ + ▸ Add package from git URL…**:

```
https://github.com/NoumanWaheed93/FormationFramework.git
```

Or add it to `Packages/manifest.json` directly:

```json
{
  "dependencies": {
    "com.artisangames.formationsystem": "https://github.com/NoumanWaheed93/FormationFramework.git"
  }
}
```

To pin a specific version, append a tag:

```
https://github.com/NoumanWaheed93/FormationFramework.git#v2.0.0
```

**Requires Unity 2021.3 or newer.** The public interfaces use C# 8 syntax, which Unity supports from
2020.2 onward.

## Quick start

Implement `IFormationMember<T>` on whatever represents a unit. `T` is your own type — the generic
parameter is what lets the formation hand you back your concrete type instead of an interface.

```csharp
using FormationSystem;
using UnityEngine;

public class Aircraft : MonoBehaviour, IFormationMember<Aircraft>
{
    // Required by the interface so the formation can return your concrete type. Always `this`.
    public Aircraft Self => this;

    // Assigned by the formation. 0 is the leader.
    public int PositionIndex { get; set; }

    // Assigned by the formation: the offset from the leader, in formation-local space.
    public Vector3 Position { get; set; }

    // Back-reference set when the member is added.
    public Formation<Aircraft> Formation { get; set; }
}
```

Then build a formation and add members to it:

```csharp
var wing = new ArrowHead<Aircraft>
{
    spacing = 20f,          // horizontal distance between adjacent members
    altitudeSpacing = 5f,   // vertical stagger between layers
};

foreach (Aircraft aircraft in squadron)
    wing.AddMember(aircraft);

// wing.leader is now squadron[0]
```

Each frame, drive your units toward the leader's transform plus their own offset:

```csharp
void Update()
{
    Transform leader = wing.leader.transform;

    foreach (Aircraft member in wing.Members)
    {
        if (member == wing.leader)
            continue;

        Vector3 offset = wing.GetMemberPositionSpaced(member.PositionIndex);
        Vector3 target = leader.position + leader.rotation * offset;

        member.transform.position = Vector3.Lerp(member.transform.position, target, Time.deltaTime * 2f);
    }
}
```

Removing a member closes the gap automatically:

```csharp
wing.RemoveMember(shotDownAircraft);
// Remaining members have new PositionIndex and Position values.
// If the leader was removed, wing.leader is now whoever took index 0.
```

## Formations

Offsets are given as unit vectors, before `spacing` is applied. `-Z` is "behind the leader" and `-Y` is
"below the leader", so a formation trails and descends away from index 0. `0.707` is `sin(45°)`, giving
the 45° swept layouts.

### `Trail<T>`

Single file, each member directly behind and below the one in front.

```
0
 1
  2
   3
```

`GetMemberPosition(i)` → `(0, -i, -i)`

### `Echelon<T>`

Diagonal line stepping right, back and down.

```
0
  1
    2
      3
```

`GetMemberPosition(i)` → `(0.707i, -i, -0.707i)`

### `ArrowHead<T>`

Leader at the point, members alternating right and left in swept layers. Extends
[`BalancedFormation<T>`](#balancedformationt).

```
   0        <- layer 0
  1 2       <- layer 1
 3   4      <- layer 2
```

Odd indices go right, even indices go left:

| index | offset                        |
| ----- | ----------------------------- |
| `0`   | `(0, 0, 0)`                   |
| odd   | `(0.707, -1, -0.707) * layer` |
| even  | `(-0.707, -1, -0.707) * layer`|

### `BattleSpread<T>`

Leader centred, members fanning straight out to both sides on the X axis. Extends
[`BalancedFormation<T>`](#balancedformationt).

```
3   1   0   2   4
```

| index | offset               |
| ----- | -------------------- |
| `0`   | `(0, 0, 0)`          |
| odd   | `(1, -1, 0) * layer` |
| even  | `(-1, -1, 0) * layer`|

### Layers

Both balanced formations think in **layers** — how far a member sits from the leader:

```
layer = isEven ? index / 2 : (index + 1) / 2
```

So indices `1` and `2` are both layer `1`, indices `3` and `4` are both layer `2`, and so on.

### Writing your own

Derive from `Formation<T>` (or `BalancedFormation<T>` for a symmetrical shape) and implement one method:

```csharp
public class Column<T> : Formation<T> where T : IFormationMember<T>
{
    public override Vector3 GetMemberPosition(int memberIndex)
    {
        CheckIndexValidity(memberIndex);   // throws ArgumentException on a negative index
        return new Vector3(0, 0, -memberIndex);
    }
}
```

## API

### `IFormationMember<T>`

```csharp
public interface IFormationMember<T> where T : IFormationMember<T>
```

| Member                  | Notes                                                                        |
| ----------------------- | ---------------------------------------------------------------------------- |
| `T Self { get; }`       | Implement as `=> this`. Lets consumers recover the concrete type.            |
| `int PositionIndex`     | Slot in the formation, `0` is the leader. Written by the formation.           |
| `Vector3 Position`      | Offset from the leader, **unscaled**. Written by the formation.               |
| `Formation<T> Formation`| Set when the member is added. Not cleared on removal.                        |

### `Formation<T>`

```csharp
public abstract class Formation<T> where T : IFormationMember<T>
```

| Member                                     | Notes                                                             |
| ------------------------------------------ | ----------------------------------------------------------------- |
| `int MemberCount`                          | Current number of members.                                        |
| `float spacing`                            | Distance between adjacent members. **Defaults to `0`.**           |
| `float altitudeSpacing`                    | Vertical distance between layers. **Defaults to `0`.**            |
| `T leader`                                 | Member at index `0`. `default(T)` when the formation is empty.     |
| `HashSet<T> Members`                       | All members. **Unordered** — sort by `PositionIndex` if you need order. |
| `void AddMember(T)`                        | Assigns the next index, sets `Position` and `Formation`, and updates `leader`. |
| `void RemoveMember(T)`                     | Removes and reshuffles the remaining indices and positions.       |
| `abstract Vector3 GetMemberPosition(int)`  | Unit offset for an index, ignoring `spacing`.                     |
| `Vector3 GetMemberPositionSpaced(int)`     | Offset with `spacing` and `altitudeSpacing` applied.              |

`GetMemberPosition` throws `ArgumentException` for a negative index.

### `BalancedFormation<T>`

```csharp
public abstract class BalancedFormation<T> : Formation<T> where T : IFormationMember<T>
```

A formation that keeps its leader centred and both flanks symmetrical. Odd indices form one side, even
indices the other, so removal has to be smarter than "shift everyone down by one": it shifts by two
within the affected side, then rebalances if one flank ends up more than one member larger than the
other. `GetMemberLayer(int index, bool isEven)` is available to subclasses.

## Spacing

`spacing` and `altitudeSpacing` are plain `float` auto-properties, so they **start at `0`**. Set them
before you read `GetMemberPositionSpaced`, or every member will sit on top of the leader.

They are applied on different axes:

```csharp
Vector3 offset = GetMemberPosition(index);
// X and Z are multiplied by `spacing`
// Y is multiplied by `altitudeSpacing`
```

Note that `member.Position` is written from `GetMemberPosition`, i.e. the **unscaled** offset. Call
`GetMemberPositionSpaced(member.PositionIndex)` when you want the world-space distance.

## Running the tests

The package ships edit-mode tests for every formation. Because tests in a package are hidden by default,
add the package to your project's `testables` in `Packages/manifest.json`:

```json
{
  "testables": [
    "com.artisangames.formationsystem"
  ]
}
```

The tests then appear under **Window ▸ General ▸ Test Runner ▸ EditMode**.

The tests use a small hand-written `TestFormationMember` stub rather than a mocking library, so no
third-party test dependency is needed.

## Upgrading from 1.x

**2.0.0 is a breaking change.** `Formation` and `IFormationMember` are now generic in the member type,
which removes the casting and `Transform` coupling the 1.x interface required.

| 1.x                                | 2.0.0                                             |
| ---------------------------------- | ------------------------------------------------- |
| `class Unit : IFormationMember`    | `class Unit : IFormationMember<Unit>`             |
| `Formation f = new Trail();`       | `Formation<Unit> f = new Trail<Unit>();`          |
| `new ArrowHead()`                  | `new ArrowHead<Unit>()`                           |
| —                                  | must now implement `T Self => this`               |
| `Vector3 velocity { get; }`        | removed — keep it on your own type                |
| `Vector3 angularVelocity { get; }` | removed — keep it on your own type                |
| `Transform Transform { get; }`     | removed — keep it on your own type                |

The assembly was also renamed from `com.artisangames.formationsystem` to `FormationSystem`. Update the
`references` array in any `.asmdef` that depends on it.

Two behaviour fixes come along with it:

- `AddMember` now sets the member's `Formation` back-reference.
- Removing the last member now clears `leader` to `default(T)` instead of leaving it dangling.

## License

[MIT](LICENSE.md) © Nouman Waheed
