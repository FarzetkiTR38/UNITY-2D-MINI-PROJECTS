# Unity 6 — 2D Game Development Agent Skill Framework

## Overview

This is a **model-agnostic Agent Skill Framework** designed for professional 2D game development in **Unity 6 (6000.3.18f1)**. It provides comprehensive rules, guidelines, templates, and examples that any LLM-based coding agent can consume to produce high-quality Unity code.

## Compatibility

This skill framework is designed to work with **any** LLM-based coding agent:

| Agent | Compatibility |
|-------|--------------|
| Claude Code | ✅ Full support |
| Gemini CLI | ✅ Full support |
| OpenAI Codex | ✅ Full support |
| Cursor | ✅ Full support |
| Windsurf | ✅ Full support |
| Roo Code | ✅ Full support |
| Cline | ✅ Full support |
| GitHub Copilot | ✅ Full support |
| Any `.agent`-compatible tool | ✅ Full support |

## Target Unity Version

```
Unity 6000.3.18f1
```

> **CRITICAL**: This framework targets Unity 6 exclusively. Do NOT use deprecated Unity APIs, legacy Input Manager patterns, or pre-URP rendering approaches.

## Technology Stack

| Technology | Version/Standard |
|-----------|-----------------|
| Unity | 6000.3.18f1 |
| Render Pipeline | Universal Render Pipeline (URP) 2D |
| Input | Input System (New) |
| UI | UI Toolkit + TextMeshPro |
| Asset Management | Addressables |
| Localization | Unity Localization Package |
| Camera | Cinemachine |
| Level Design | Tilemap |
| Physics | 2D Physics (Rigidbody2D, Collider2D) |
| Architecture | ScriptableObject-driven |
| Code Organization | Assembly Definitions |
| C# Version | Modern C# (10+) |

## Directory Structure

```
.agent/
├── README.md                        ← You are here
├── SKILL.md                         ← Core skill definition and triggers
├── UNITY_RULES.md                   ← Unity 6 specific rules and constraints
├── UNITY_CODING_GUIDELINES.md       ← C# coding standards for Unity
├── PROJECT_STRUCTURE.md             ← Project folder and asset organization
├── ARCHITECTURE_GUIDE.md            ← Design patterns and architecture
├── NAMING_CONVENTIONS.md            ← Naming rules for all asset types
├── SCRIPT_TEMPLATE.md               ← Script templates with documentation
├── SCRIPTABLEOBJECT_GUIDE.md        ← ScriptableObject usage patterns
├── PERFORMANCE_GUIDE.md             ← Performance optimization rules
├── UI_GUIDE.md                      ← UI development guidelines
├── GAMEPLAY_GUIDE.md                ← 2D gameplay systems reference
├── DEBUG_GUIDE.md                   ← Debugging and profiling guide
├── TESTING_GUIDE.md                 ← Testing strategies and patterns
├── CHECKLIST.md                     ← Pre-commit and review checklists
├── COMMON_MISTAKES.md               ← Anti-patterns and common errors
├── PROMPT_RULES.md                  ← Agent behavioral rules
├── EXAMPLES/
│   ├── Templates/
│   │   ├── MonoBehaviourTemplate.cs
│   │   ├── ScriptableObjectTemplate.cs
│   │   ├── StateMachineTemplate.cs
│   │   ├── EventChannelTemplate.cs
│   │   ├── ObjectPoolTemplate.cs
│   │   └── ServiceTemplate.cs
│   ├── Snippets/
│   │   ├── PlayerController2D.cs
│   │   ├── HealthSystem.cs
│   │   ├── SaveSystem.cs
│   │   ├── AudioManager.cs
│   │   ├── ObjectPool.cs
│   │   └── InputHandler.cs
│   └── Rules/
│       ├── assembly_definition_rules.md
│       ├── prefab_rules.md
│       └── inspector_rules.md
```

## How to Use

### For Agent Developers

1. Place this `.agent/` folder in the root of your Unity project.
2. The agent will automatically discover and load the skill definitions.
3. All guidelines, rules, and templates are available as context for code generation.

### For Manual Reference

Each `.md` file is self-contained and can be read independently. Start with:
1. `SKILL.md` — Understand what this skill covers
2. `UNITY_RULES.md` — Core Unity 6 rules
3. `ARCHITECTURE_GUIDE.md` — Design patterns to follow
4. `CHECKLIST.md` — Quick reference for code reviews

## Design Principles

1. **Model-Agnostic**: No model-specific prompting. Pure knowledge and rules.
2. **Production-Ready**: Every guideline is battle-tested for shipping games.
3. **Unity 6 Native**: Leverages Unity 6 features, not legacy workarounds.
4. **2D Focused**: Every example and pattern is tailored for 2D game development.
5. **Performance-First**: GC-conscious, pool-friendly, profile-aware.
6. **Scalable Architecture**: Patterns that grow with your project.

## License

This skill framework is provided for use in Unity game development projects. Adapt and extend as needed for your specific project requirements.
