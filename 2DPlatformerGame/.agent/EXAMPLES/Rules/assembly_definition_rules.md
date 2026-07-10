# Assembly Definition Rules

## Overview

Assembly Definitions (`.asmdef`) are mandatory for all Unity 6 projects. They enforce dependency boundaries, speed up compilation, and prevent circular dependencies.

---

## 1. Required Assemblies

| Assembly Name | Path | Purpose |
|--------------|------|---------|
| `GameName.Runtime.Core` | `Scripts/Runtime/Core/` | Interfaces, events, patterns, utilities |
| `GameName.Runtime.Gameplay` | `Scripts/Runtime/Gameplay/` | Player, enemies, combat, items, environment |
| `GameName.Runtime.Systems` | `Scripts/Runtime/Systems/` | Audio, save, scene, input, dialogue, quest |
| `GameName.Runtime.UI` | `Scripts/Runtime/UI/` | Screens, components, managers |
| `GameName.Editor` | `Scripts/Editor/` | Custom inspectors, tools, property drawers |
| `GameName.Tests.EditMode` | `Tests/EditMode/` | Unit tests (NUnit) |
| `GameName.Tests.PlayMode` | `Tests/PlayMode/` | Integration tests (Play Mode) |

---

## 2. Dependency Matrix

```
                          Core  Gameplay  Systems  UI  Editor  Tests
Core                       -      ✗        ✗       ✗    ✗       ✗
Gameplay                   ✓      -        ✗       ✗    ✗       ✗
Systems                    ✓      ✗        -       ✗    ✗       ✗
UI                         ✓      ✗        ✓       -    ✗       ✗
Editor                     ✓      ✓        ✓       ✓    -       ✗
Tests.EditMode             ✓      ✓        ✗       ✗    ✗       -
Tests.PlayMode             ✓      ✓        ✓       ✓    ✗       -

✓ = Allowed dependency
✗ = Forbidden dependency
- = Self
```

---

## 3. Configuration Rules

### 3.1 Runtime Assembly

```json
{
    "name": "GameName.Runtime.Core",
    "rootNamespace": "GameName.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 3.2 Editor Assembly

```json
{
    "name": "GameName.Editor",
    "rootNamespace": "GameName.Editor",
    "references": [
        "GameName.Runtime.Core",
        "GameName.Runtime.Gameplay",
        "GameName.Runtime.Systems",
        "GameName.Runtime.UI"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 3.3 Test Assembly

```json
{
    "name": "GameName.Tests.EditMode",
    "rootNamespace": "GameName.Tests.EditMode",
    "references": [
        "GameName.Runtime.Core",
        "GameName.Runtime.Gameplay",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

## 4. Rules

1. **Every script folder MUST have an asmdef** — No loose scripts outside assembly boundaries.
2. **Core never depends upward** — Core is the foundation; it knows nothing about Gameplay, Systems, or UI.
3. **Gameplay never depends on UI** — Gameplay raises events; UI listens. No direct coupling.
4. **Runtime never depends on Editor** — Editor code is stripped from builds.
5. **Use rootNamespace** — Matches the assembly name for auto-namespace suggestions.
6. **Test assemblies use defineConstraints** — `UNITY_INCLUDE_TESTS` ensures they're excluded from builds.
7. **Editor assemblies use includePlatforms** — `["Editor"]` ensures they're excluded from builds.
