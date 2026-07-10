# Animation Rules

## Overview

Rules for implementing 2D animations in Unity 6 projects.

---

## 1. Animator Setup Rules

| Rule | Details |
|------|---------|
| One Animator Controller per entity type | `PlayerAnimator.controller`, `SlimeAnimator.controller` |
| Use Override Controllers for variants | `SlimeFire_AnimOverride.overrideController` |
| Animator on child object with SpriteRenderer | Not on the root physics object |
| Cache all parameter hashes | `static readonly int AnimHash_X = Animator.StringToHash("X")` |
| Use bool parameters for states | `IsGrounded`, `IsRunning`, `IsWallSliding` |
| Use float parameters for blending | `Speed`, `VelocityY` |
| Use trigger parameters for one-shots | `Jump`, `Attack`, `Hurt`, `Die` |

---

## 2. Parameter Naming

| Parameter | Type | Used For |
|-----------|------|----------|
| `Speed` | Float | Horizontal speed (absolute value) |
| `VelocityY` | Float | Vertical velocity for air blend |
| `IsGrounded` | Bool | Ground contact state |
| `IsRunning` | Bool | Whether actively moving |
| `IsWallSliding` | Bool | Wall slide state |
| `IsDead` | Bool | Death state (locked) |
| `Jump` | Trigger | Jump initiation |
| `Attack` | Trigger | Attack initiation |
| `Hurt` | Trigger | Damage reaction |
| `Die` | Trigger | Death initiation |
| `AttackIndex` | Int | Which attack in combo |

---

## 3. Code Integration

```csharp
// Cache hashes as static readonly
private static readonly int AnimHash_Speed = Animator.StringToHash("Speed");
private static readonly int AnimHash_VelocityY = Animator.StringToHash("VelocityY");
private static readonly int AnimHash_IsGrounded = Animator.StringToHash("IsGrounded");
private static readonly int AnimHash_Jump = Animator.StringToHash("Jump");
private static readonly int AnimHash_Attack = Animator.StringToHash("Attack");
private static readonly int AnimHash_Hurt = Animator.StringToHash("Hurt");
private static readonly int AnimHash_Die = Animator.StringToHash("Die");

// Update in Update() — NOT FixedUpdate
private void UpdateAnimator()
{
    if (_animator == null) return;

    _animator.SetFloat(AnimHash_Speed, Mathf.Abs(_moveInput.x));
    _animator.SetFloat(AnimHash_VelocityY, _rb.linearVelocity.y);
    _animator.SetBool(AnimHash_IsGrounded, _isGrounded);
}

// Triggers from events
private void OnJumped()
{
    _animator.SetTrigger(AnimHash_Jump);
}

private void OnDamaged(int amount)
{
    _animator.SetTrigger(AnimHash_Hurt);
}
```

---

## 4. Transition Rules

| Rule | Details |
|------|---------|
| Set transition duration to 0 for pixel art | No blending between pixel art states |
| Use "Has Exit Time" sparingly | Only for attack recovery, death animations |
| Interrupt Source: Current State | Allows immediate interrupts for responsive gameplay |
| Ordered Interruption: checked | Ensures priority of important transitions |
| Any State → Hurt | Damage reaction interrupts everything |
| Any State → Die | Death interrupts everything |

---

## 5. Animation Events

| Rule | Details |
|------|---------|
| Use for gameplay-critical timing | Attack damage frame, footstep sounds |
| Method must exist on same GameObject | Or child with the Animator |
| Name format: `OnAnim_[EventName]` | `OnAnim_AttackHit`, `OnAnim_Footstep` |
| No string parameters in events | Use int indices or parameterless methods |
| Document events in script XML | List all animation events the script handles |

```csharp
// Animation event callbacks
/// <summary>
/// Called by animation event on the attack damage frame.
/// Performs the actual damage check against targets in range.
/// </summary>
public void OnAnim_AttackHit()
{
    _meleeAttack.PerformAttack();
}

/// <summary>
/// Called by animation event on footstep frames.
/// Plays a random footstep sound.
/// </summary>
public void OnAnim_Footstep()
{
    if (_audioCollection.TryGetClip(0, out var clip, out float vol, out float pitch))
    {
        _audioSource.pitch = pitch;
        _audioSource.PlayOneShot(clip, vol);
    }
}
```

---

## 6. Sprite Rules

| Rule | Details |
|------|---------|
| Consistent Pixels Per Unit | All sprites in a category use same PPU |
| Power-of-2 textures preferred | Better GPU performance |
| Use Sprite Atlas | Group by category (Characters, Environment, UI) |
| Pivot set correctly | Character sprites: bottom-center |
| Filter Mode: Point for pixel art | Bilinear for HD 2D |
| Compression: None for pixel art | Crunch for HD 2D |
| Generate Mip Maps: OFF | Not needed for 2D games |
| Read/Write: OFF | Saves memory |
