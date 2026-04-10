# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**PathOfTheInfected** — a Unity 2D pixel-art action game with a custom 2D platformer controller, Verlet-based cloth physics (scarf/hair), a state machine-driven combat system, and an enemy AI system with pathfinding and spotting.

- **Unity version:** 2022.x (serializedVersion: 28)
- **Platform:** Windows (build target 19)
- **Resolution:** 1920x1080
- **ActiveInputHandler:** New Input System
- **Color space:** Linear

## Development Workflow

This is a Unity project — there are no build scripts, CI/CD pipelines, or automated test runners. Development is done inside the Unity Editor. There is no `make`, `npm`, or script-based test infrastructure.

- **Open the project in Unity Editor** to test and iterate
- **Version control:** Git, branch naming follows `Feat_<FeatureName>` convention (e.g., `Feat_ExtendingAttacksAndWeaponSystem`)
- Main branch: `main`

## Assembly Definitions (.asmdef)

The project uses 4 custom assemblies with dependency graph:

| Assembly | GUID | Description |
|----------|------|-------------|
| `TidiSystems` | `3b6257f84980fdc428a1d48db325deb5` | Core 2D movement, animation, physics, states — foundational layer |
| `TidiGameplayMessaging` | `75469ad4d38634e559705d17036d5f35` | Typed event/message bus for decoupled communication |
| `TidiPathFinding` | `f5cdf24af6a043b897086d4de2582826` | Enemy pathfinding subsystem |
| `ClothSystem` | — | Verlet-based 2D cloth/scarf simulation (depends on `TidiSystems`) |
| `PathOfTheInfected - Core` | — | Game-specific code (combat, enemies, player, UI) — depends on all above |

When adding new scripts, ensure they are placed in the correct assembly folder or create a new `.asmdef` if the dependency boundary warrants it.

## Code Architecture

### 1. Custom 2D Movement Component (`TidiMovementComponent2D/scripts/`)

Reusable platformer controller framework with:

- **`Core/PlayerSm.cs`** — Central player singleton (`PlayerSm.Instance`). Manages the state machine, input, movement stats, and exposes properties like `IsFacingRight`, `IsGrounded`, `velocity`, `IsDashing`, etc. Uses a singleton pattern.
- **`States/Player*StateSM.cs`** — ~16 player states (Idle, Walk, Run, Dash, JumpAscending, JumpCut, Falling, WallSlide, WallJump*, Slide, Crouch, etc.). Each state is a class that hooks into state lifecycle methods.
- **`Physics/MovementControllerSm.cs`** — Low-level movement: velocity, gravity, ground/wall detection, coyote time.
- **`Animation/TidiAnimInstance.cs`** — Animation state machine base class, extended by `POIAnimInstance`.
- **`Animation/AnimationHandle.cs`** — Animation handle/manager for tracking playback and completion events.
- **`Misc/GhostTrail.cs`** — Ghost trail visual effect.

### 2. Animation System

- **`TidiAnimInstance`** (base) → **`POIAnimInstance`** (game-specific) — manages animation state machines with `StandingState` and `InAirState` animation states.
- Animation clips are referenced on `POIAnimInstance` (idle, walk, run, dash, punch, jump, slide, wallSlide, etc.).
- Uses hash-based animation triggering via `Animator.StringToHash`.

### 3. Combat System (`PathOfTheInfected/Scripts/`)

- **`PlayerCombat.cs`** — Player combat orchestrator. Handles input capture, attack buffering, recovery timers, and coordinates attack execution with animation events.
- **`Player/Combat/Attacks/PlayerAttackSoBase`** — ScriptableObject-based attack definitions. Current attack: punch.
- **`Combat/AttackDefinition/PlayerAttackDefinition.cs`** — SO extending base `AttackDefinition` with `attackBufferTime`, `attackRange`, `firstHitDamageBoost`.
- **`PlayerRBCSubsystem`** — Reset-Based Combat subsystem (combo-based cooldown resets).
- **`PlayerComboSubsystem`** — Combo tracking and multipliers (`maxComboHitMultiplier`, `maxComboSpeedMultiplier`).
- **`Combat/Subsystems/CombatSubsystem.cs`** — Base class for combat subsystems.

### 4. Gameplay Messaging (`TidiGameplayMessaging/Core/`)

- **`TidiGameplayMessagingSubsystem`** — Singleton typed message bus. Subscribe via `Listen<TChannel, TPayload>(callback)`, publish via `Publish<TChannel, TPayload>(payload)`. Supports both payload-carrying and signal-only channels. Uses lazy initialization.

### 5. Cloth Physics (`TidiMovementComponent2D/scripts/Cloth/`)

- **`ClothController.cs`** — Verlet integration cloth simulation. Fixed-timestep accumulator, configurable substeps, distance constraints, bend constraints, shape constraints, wind forces, turn impulse on player flip. Renders via instantiating segment prefabs.
- **`Data/ClothPoint.cs`** — Simple data class (`position`, `previousPosition`).

### 6. Enemy AI (`PathOfTheInfected/Scripts/Enemy/`)

- **`EnemySpottable.cs`** — Spotting/targeting system for enemies.
- **`EnemyDamagable.cs`** — Damageable component for enemies.
- Enemy prefabs exist under `Prefabs/Enemy/` (e.g., `FlyingPuncherTest.prefab`).

## Key Patterns

- **Singletons everywhere:** `PlayerSm.Instance`, `POIAnimInstance.Instance`, `TidiGameplayMessagingSubsystem.Instance`
- **State machines:** Both movement (player states) and animations use state machine patterns with `StateMachine.InitializeDefaultState()`, `CurrentState.StateUpdate()`, and queued state changes.
- **Bit flags:** Combat system uses `CombatIntentFlags` and `CombatFlags` enums with `[Flags]` for tracking active combat states.
- **ScriptableObject data:** Attack definitions and other config are driven by ScriptableObjects created in the Unity Editor.
- **Animation events:** Combat integrates with animation via `AnimationHandle.OnAnimationEnded` callback and `OnAnimationAttackMessage`.

## Common File Locations

| Task | Go to |
|------|-------|
| Player movement/physics | `TidiMovementComponent2D/scripts/Core/PlayerSM.cs`, `states/`, `physics/` |
| Player combat | `PathOfTheInfected/Scripts/Player/Combat/PlayerCombat.cs` |
| Attack definitions | `PathOfTheInfected/Scripts/Combat/Attack Definition/` |
| Animation logic | `TidiMovementComponent2D/scripts/Animation/TidiAnimInstance.cs`, `PathOfTheInfected/Scripts/Animation/POIAnimInstance.cs` |
| Enemy AI | `PathOfTheInfected/Scripts/Enemy/` |
| Cloth/scarf physics | `TidiMovementComponent2D/scripts/Cloth/ClothController.cs` |
| Game events/messaging | `TidiGameplayMessaging/Core/TidiGameplayMessagingSubsystem.cs` |
| Main scene | `PathOfTheInfected/Scenes/SampleScene.unity` |
| Prefabs | `PathOfTheInfected/Prefabs/` |
