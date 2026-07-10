# Agent Behavioral Rules (Prompt Rules)

## Overview

This document defines the mandatory behavioral rules for ANY AI coding agent working on this Unity project. These rules are model-agnostic and must be followed regardless of which LLM or coding assistant is being used.

---

## 1. Before Writing Any Code

### 1.1 Analyze First

Before writing or modifying any code, the agent MUST:

1. **Read the existing codebase** — Understand what already exists
2. **Identify the architecture** — Determine which patterns are in use
3. **Check for existing solutions** — Don't recreate what already exists
4. **Understand naming conventions** — Match the project's existing style
5. **Verify dependencies** — Know what assemblies and packages are available
6. **Check file structure** — Know where new files should be placed

### 1.2 Decision Flow

```
User Request
     │
     ▼
┌────────────────────────────┐
│ Does this code exist?      │
│                            │
│ YES → Modify existing code │
│ NO  → Continue below       │
└────────────────────────────┘
     │
     ▼
┌────────────────────────────┐
│ Does a similar system      │
│ exist in the project?      │
│                            │
│ YES → Extend existing      │
│ NO  → Create new file      │
└────────────────────────────┘
     │
     ▼
┌────────────────────────────┐
│ Where should this file go? │
│                            │
│ Follow PROJECT_STRUCTURE   │
│ Follow NAMING_CONVENTIONS  │
│ Follow assembly boundaries │
└────────────────────────────┘
```

---

## 2. Code Generation Rules

### 2.1 Mandatory Output Quality

Every piece of generated code MUST:

```
□ Compile without errors or warnings
□ Follow all rules in UNITY_CODING_GUIDELINES.md
□ Follow all rules in NAMING_CONVENTIONS.md
□ Follow all rules in UNITY_RULES.md
□ Include complete XML documentation
□ Include Inspector attributes ([Header], [Tooltip], [SerializeField])
□ Include OnValidate for reference validation
□ Include null safety checks
□ Include proper namespace
□ Include file header comment
□ Be production-ready (no TODOs, no placeholders, no "add logic here")
□ Be fully functional (every method implemented, every path handled)
```

### 2.2 Never Do

```
❌ Never produce placeholder code:
   // TODO: implement this
   // Add your logic here
   // ...
   throw new NotImplementedException();

❌ Never produce incomplete methods:
   public void DoSomething()
   {
       // Implementation needed
   }

❌ Never skip error handling:
   _references.GetComponent<T>(); // No null check

❌ Never use deprecated APIs:
   FindObjectOfType<T>()
   Input.GetKey()
   Resources.Load()

❌ Never hardcode values:
   transform.position += Vector3.right * 5f;

❌ Never use public fields:
   public float speed = 5f;

❌ Never create unnecessary Singletons

❌ Never ignore existing project conventions

❌ Never break existing functionality when adding new features

❌ Never produce code that causes GC allocation in hot paths
```

### 2.3 Always Do

```
✅ Always produce complete, compilable code
✅ Always include XML documentation for public members
✅ Always validate SerializeField references
✅ Always use named constants instead of magic numbers
✅ Always cache component references in Awake
✅ Always subscribe in OnEnable, unsubscribe in OnDisable
✅ Always use TryGetComponent instead of GetComponent
✅ Always use Input System (New) for input handling
✅ Always use [SerializeField] private instead of public
✅ Always include [Tooltip] on serialized fields
✅ Always match existing naming and formatting conventions
✅ Always consider performance implications
✅ Always provide the complete file content (not snippets)
```

---

## 3. Response Format Rules

### 3.1 Code Responses

When generating code:

1. **Complete file** — Always provide the entire file, not just a snippet
2. **No summaries** — Don't summarize what the code does instead of writing it
3. **No placeholders** — Every line must be functional production code
4. **No options without recommendation** — If multiple approaches exist, recommend one with justification
5. **File path** — Always state where the file should be placed

### 3.2 Explanation Responses

When explaining code or architecture:

1. **Be specific** — Reference actual class names, method names, and file paths
2. **Include code examples** — Show, don't just tell
3. **Explain why** — Don't just say what to do, explain the reasoning
4. **Reference guides** — Point to relevant .agent/ guide files

---

## 4. Modification Rules

### 4.1 When Modifying Existing Code

```
1. Read the ENTIRE file before making changes
2. Understand the class's role in the architecture
3. Maintain existing:
   - Code style and formatting
   - Naming conventions
   - Region organization
   - XML documentation style
   - Event patterns
   - Dependency patterns
4. Do NOT:
   - Rename existing public members (breaks references)
   - Change existing method signatures (breaks callers)
   - Remove existing functionality
   - Add new dependencies without justification
   - Refactor code that isn't related to the change
5. DO:
   - Add new methods/fields as needed
   - Update XML documentation if behavior changes
   - Add OnValidate checks for new references
   - Follow the existing class structure
```

### 4.2 Refactoring Rules

```
ONLY refactor when:
- Explicitly asked to refactor
- A bug fix requires architectural change (explain why)
- New feature integration requires restructuring (explain why)

NEVER refactor:
- Working code for "cleanliness" without being asked
- Code in other files not related to the current task
- Established patterns for "better" alternatives
```

---

## 5. Project Awareness

### 5.1 Unity Version Awareness

```
This project uses Unity 6000.3.18f1.

ALWAYS verify:
- API is available in Unity 6
- No deprecated API usage
- Unity 6 specific features used where beneficial (Awaitable, etc.)
- Rigidbody2D uses .linearVelocity, not .velocity
- Cinemachine uses Unity.Cinemachine namespace (3.x), not Cinemachine (2.x)
```

### 5.2 Package Awareness

```
Available Packages (verify before using):
- Input System (New)
- Universal Render Pipeline (URP)
- Cinemachine (3.x)
- TextMeshPro
- Addressables
- Localization
- 2D Animation
- 2D Tilemap
- 2D SpriteShape
- 2D Pixel Perfect
- Burst
- Collections
- Mathematics
- Unity Test Framework
```

### 5.3 Architecture Awareness

```
Before adding code, verify:
- Which assembly this code belongs to (Core, Gameplay, Systems, UI)
- What interfaces should be implemented
- What events should be raised
- What ScriptableObject data should drive configuration
- What existing services can be reused
- What pool should be used for object lifecycle
```

---

## 6. Communication Style

### 6.1 When Uncertain

```
If uncertain about:
- Existing architecture → READ the code first, then ask
- Design decision → Recommend one option with justification, note alternatives
- Unity 6 API availability → Verify in documentation before using
- Performance impact → Default to the safer (less allocation) approach
```

### 6.2 Explanations

```
When explaining changes:
- State WHAT was changed
- State WHY it was changed
- State WHERE the change is located
- Note any DEPENDENCIES or SIDE EFFECTS
- Mention any FOLLOW-UP ACTIONS needed
```

---

## 7. Error Recovery

### 7.1 If a Mistake Is Made

```
1. Acknowledge the error clearly
2. Explain what went wrong and why
3. Provide the corrected code immediately
4. Note how to prevent the same mistake
```

### 7.2 If Conflicting Requirements

```
1. Identify the conflict
2. Explain both sides
3. Recommend the approach that:
   - Is more performant
   - Is more maintainable
   - Follows Unity best practices
   - Is simpler to implement correctly
4. Justify the recommendation
```
