# Path of the Infected

A 2D pixel-art action platformer built in Unity for a Game Development class.

## 🎮 Features

### Player System

#### Custom 2D Platformer Controller
A state machine-driven movement controller with ~16 discrete states:

| State | Description |
|-------|-------------|
| **Idle/Walk/Run** | Ground movement with velocity-based transitions |
| **Jump Ascending/Cut** | Variable jump height with apex tracking |
| **Falling** | Airborne state with gravity control |
| **Dash** | Horizontal burst with cooldown |
| **Dash Fast Fall** | Cancel vertical velocity during dash |
| **Dash Cancel Vertical** | Stop upward momentum mid-dash |
| **Wall Slide** | Slow descent along walls |
| **Wall Jump (Asc/Cut/Fast Fall)** | Three variants based on input |
| **Slide** | Quick ground-level evasion |
| **Crouch** | Defensive stance |

**Key Files:**
- `Assets/TidiMovementComponent2D/scripts/Core/PlayerSM.cs` — Central singleton (`PlayerSm.Instance`) managing state machine, input, movement stats
- `Assets/TidiMovementComponent2D/scripts/States/Player*StateSM.cs` — Individual state implementations
- `Assets/TidiMovementComponent2D/scripts/Physics/MovementControllerSM.cs` — Low-level physics: velocity, gravity, ground/wall detection, coyote time

#### Animation System
- **`TidiAnimInstance`** (base) → **`POIAnimInstance`** (game-specific)
- Hash-based animation triggering via `Animator.StringToHash`
- Animation states: `StandingState`, `InAirState`
- Blended combat animations with directional input support

**Key Files:**
- `Assets/TidiMovementComponent2D/scripts/Animation/TidiAnimInstance.cs`
- `Assets/PathOfTheInfected - Core/Scripts/Animation/POIAnimInstance.cs`

#### Combat System
Reset-Based Combat (RBC) with combo multipliers and perk subsystems:

| Component | Description |
|-----------|-------------|
| **Input Capture** | Attack buffering with configurable window |
| **Timeline-Based Attacks** | Startup → Active → Recovery phases |
| **BlendSpace2D** | Directional punch animations based on stick input |
| **Combo Subsystem** | Hit counters, speed/damage multipliers |
| **RBC Subsystem** | Cooldown resets on successful hits |
| **Perk Subsystem** | Passive bonuses from collected perks |

**Combat Flags:** `CombatIntentFlags` (input intents) and `CombatFlags` (active states) using `[Flags]` bitfields

**Key Files:**
- `Assets/PathOfTheInfected - Core/Scripts/Player/Combat/PlayerCombat.cs` — Main orchestrator
- `Assets/PathOfTheInfected - Core/Scripts/Combat/Subsystems/` — Modular subsystems
- `Assets/PathOfTheInfected - Core/Scripts/Player/Combat/Attacks/` — ScriptableObject attack definitions

#### Health & Damage
- **PlayerHealth** — Damage reception, hit response, UI message broadcasting
- **HitStopManager** — Frame-freeze on impact for juice
- **EnemyHealth** — Enemy damage handling with death events

### Enemy AI System

#### State Machine Architecture
```
EnemyStateMachine
├── NoSpottableDetectedState → Wander/Patrol
├── SpottableDetectedState → Aggro/Chase
└── SpottableInAttackRangeState → Attack
```

#### Enemy Types

| Enemy | Behavior |
|-------|----------|
| **FlyingPuncher** | Flying movement personality, melee punch attack |
| **MeleePuncher** | Ground-based chaser with touch/punch attacks |
| **RangedEnemy** | Projectile-based attacks from distance |

#### Brain System
- **EnemyBrainBase** — Decision-making core
- **EnemyFlyingBrainBase** — Flying-specific logic
- **MovementPersonality** — Configurable movement params (speed, acceleration, aggro range)

#### Spotting System
- **EnemySpottable** — Player detection via raycasting/distance checks
- **PlayerSpot** — Player-side spot marker for enemy AI
- **ISpottable** — Interface for detectable entities

**Key Files:**
- `Assets/PathOfTheInfected - Core/Scripts/Enemy/` — All enemy code
- `Assets/PathOfTheInfected - Core/Scripts/Enemy/Enemy brains/` — Brain implementations
- `Assets/PathOfTheInfected - Core/Scripts/Enemy/States/` — State definitions

### Boss System

| Component | Description |
|-----------|-------------|
| **BossBrain** | Phase-based Boss AI |
| **BossPhase** | Multi-phase combat with transition logic |
| **BossCombatState** | Attack patterns per phase |
| **BossHealth** | Phase triggers on HP thresholds |

### Cloth Physics System

Verlet integration-based 2D cloth simulation for scarf/hair:

| Feature | Description |
|---------|-------------|
| **Fixed Timestep** | Accumulator with configurable substeps |
| **Distance Constraints** | Segment-to-segment length preservation |
| **Bend Constraints** | Angular resistance for stiffness |
| **Shape Constraints** | Anchor points for structure |
| **Wind Forces** | Environmental perturbation |
| **Turn Impulse** | Flick on player direction change |

**Key Files:**
- `Assets/TidiMovementComponent2D/scripts/Cloth/ClothController.cs` — Main simulation
- `Assets/TidiMovementComponent2D/scripts/Cloth/Data/ClothPoint.cs` — Point data structure

### Visual Effects

| Effect | Description |
|--------|-------------|
| **GhostTrail** | Afterimage trail during dash |
| **Jump Particles** | Ground dust on jump |
| **WallSlide Particles** | Wall friction sparks |
| **Speed Particles** | Motion blur effect at high velocity |
| **Land Particles** | Impact dust on landing |

### Modular UI Framework (UI Toolkit)

Component-based UI architecture using Unity's UI Toolkit:

| Layer | Responsibility |
|-------|----------------|
| **UIView** | VisualElement subclass with Show()/Hide() lifecycle |
| **UIScreen<T>** | Controller logic (Init, Start, Tick, FixedTick, Dispose) |
| **UIRouter** | Routes controllers to views, manages lifecycle |
| **UIComponent** | Decorator pattern for reusable UI behaviors |

**Examples:**
- `ControllerIconDecorator` — Dynamically swaps controller button icons
- `UIButtonComponent` — Abstracts button click handling

**Key Files:**
- `Assets/TidiModularUISystem/Scripts/Core/` — Core framework
- `Assets/PathOfTheInfected - UI/Scripts/` — Game-specific UI implementation
- `Assets/PathOfTheInfected - UI/Scripts/Router/POIRouter.cs` — Game UI router

### Gameplay Messaging System

Typed event bus for decoupled communication:

```csharp
// Publish
TidiGameplayMessagingSubsystem.Instance.Broadcast<OnPlayerHealthChangedUI, HealthPayload>(payload);

// Subscribe
TidiGameplayMessagingSubsystem.Instance.Listen<PlayerHealthChannel, HealthPayload>(OnHealthChanged);
```

**Features:**
- Zero-boxing payload dispatch via `ArrayPool<T>`
- Re-entrant safe (queued delivery)
- Signal-only channels (no payload) and payload channels
- IDisposable unsubscription tokens

**Key Files:**
- `Assets/TidiGameplayMessaging/Core/TidiGameplayMessagingSubsystem.cs`
- `Assets/GlobalMessages/` — Message channel definitions

### Garbage Collection Manager

Manual GC control to prevent runtime stutters:

| Feature | Description |
|---------|-------------|
| **Disabled Auto-GC** | Full manual control in production builds |
| **Heap Threshold** | Adaptive threshold based on system RAM |
| **Stutter Detection** | Monitors GC frequency, increases threshold on stutter |
| **Scene Transition Cleanup** | Full compacting GC on scene unload |
| **Memory Profiling** | Exposes `GetMemoryUsage()`, `GetMonoHeapUsage()` |

**Key Files:**
- `Assets/TidiGC/TidiGCManager.cs` — Singleton GC manager

### Input System

Unity New Input System with layered managers:

| Layer | Description |
|-------|-------------|
| **POIInputManager** | Raw input reading (Punch, Pause) |
| **POIInputCommandManager** | Command-layer abstraction for gameplay |
| **InputManager** (base) | Generic input action handling |

**Key Files:**
- `Assets/PathOfTheInfected - Core/Scripts/Input/POIInputManager.cs`
- `Assets/TidiMovementComponent2D/scripts/Misc/InputManager.cs`

### Object Pooling

Generic object pooler for projectiles, particles, and prefabs:

- `ObjectPooler` — Base pool manager with spawn/despawn
- `TidiGenericObjectPooling` — Runtime pooling for dynamic objects

**Key Files:**
- `Assets/TidiMovementComponent2D/scripts/Core/ObjectPooler.cs`
- `Assets/TidiMovementComponent2D/scripts/TidiGenericObjectPooling/ObjectPoolManager.cs`

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|------------|
| **Engine** | Unity 6000.3.6f1 (Unity 6) |
| **Language** | C# (.NET) |
| **Input** | Unity New Input System |
| **UI** | Unity UI Toolkit (VisualElements) |
| **Physics** | Unity 2D Physics (BoxCollider2D, Rigidbody2D) |
| **Animation** | Unity Animator with Playables (TidiAnimationDriver) |
| **Resolution** | 1920x1080 (Linear color space) |
| **Platform** | Windows (Build Target 19) |

---

## 📦 Assembly Structure

The project uses 11 custom `.asmdef` assemblies with clear dependency boundaries:

| Assembly | GUID | Description | Dependencies |
|----------|------|-------------|--------------|
| `TidiSystems` | `3b6257f84980fdc428a1d48db325deb5` | Core 2D movement, animation, physics, states | — |
| `TidiGameplayMessaging` | `75469ad4d38634e559705d17036d5f35` | Typed event/message bus | — |
| `TidiPathFinding` | `f5cdf24af6a043b897086d4de2582826` | Enemy AI pathfinding | — |
| `TidiPatjFindingEditor` | — | Editor tooling for pathfinding | TidiPathFinding |
| `TidiGC` | — | Garbage collection management | — |
| `TidiModularUISystem` | — | Component-based UI framework | — |
| `ClothSystem` | — | Verlet cloth simulation | TidiSystems |
| `GlobalMessages` | — | Message channel definitions | TidiGameplayMessaging |
| `PathOfTheInfected - Core` | — | Game-specific mechanics and behaviors | All above |
| `PathOfTheInfected - Gameplay` | — | Gameplay managers and systems | Core assemblies |
| `PathOfTheInfected - UI` | — | UI implementation | TidiModularUISystem |

---

## 📁 Project Structure

```
PathOfTheInfected/
├── Assets/
│   ├── TidiMovementComponent2D/          # Reusable 2D controller framework
│   │   ├── scripts/
│   │   │   ├── Core/                     # PlayerSM singleton, state machine
│   │   │   ├── States/                   # ~16 player states
│   │   │   ├── Physics/                  # MovementController, collision
│   │   │   ├── Animation/                # AnimInstance, BlendSpaces, Playables
│   │   │   ├── Cloth/                    # Verlet cloth simulation
│   │   │   ├── Misc/                     # GhostTrail, InputManager, Pooling
│   │   │   └── Tweening/                 # TidiTween
│   │   └── TidiSystems.asmdef
│   │
│   ├── TidiGameplayMessaging/            # Typed event bus
│   │   ├── Core/                         # Messaging subsystem
│   │   └── TidiGameplayMessaging.asmdef
│   │
│   ├── TidiPathFinding/                  # Enemy navigation
│   │   ├── Core/                         # Pathfinding engine
│   │   ├── Editor/                       # Editor tools
│   │   └── TidiPathFinding.asmdef
│   │
│   ├── TidiGC/                           # GC management
│   │   ├── TidiGCManager.cs
│   │   └── TidiGC.asmdef
│   │
│   ├── TidiModularUISystem/              # UI Toolkit framework
│   │   ├── Scripts/Core/
│   │   │   ├── Screens/                  # UIScreen controllers
│   │   │   ├── Views/                    # UIView components
│   │   │   ├── Routers/                  # UIRouter lifecycle
│   │   │   └── Components/               # UIComponent decorators
│   │   └── TidiModularUISystem.asmdef
│   │
│   ├── GlobalMessages/                   # Message channel definitions
│   │   └── GlobalMessages.asmdef
│   │
│   └── PathOfTheInfected - Core/         # Game-specific code
│       ├── Scripts/
│       │   ├── Player/
│       │   │   ├── Combat/               # PlayerCombat, attacks, subsystems
│       │   │   └── Health/               # PlayerHealth
│       │   ├── Combat/
│       │   │   ├── Attack Definition/    # ScriptableObject attacks
│       │   │   ├── Subsystems/           # RBC, Combo, Perks
│       │   │   └── HitDispatcher.cs      # Damage routing
│       │   ├── Enemy/
│       │   │   ├── Enemy brains/         # AI decision making
│       │   │   ├── States/               # Enemy state machines
│       │   │   ├── Attack/               # Melee, ranged, touch attacks
│       │   │   ├── Health/               # EnemyHealth
│       │   │   ├── Projectiles/          # Projectile base
│       │   │   └── Interfaces/           # ISpottable, IEnemyMoveable
│       │   ├── Boss/
│       │   │   ├── Brains/               # BossBrain
│       │   │   ├── Phases/               # BossPhase transitions
│       │   │   └── States/               # Boss combat states
│       │   ├── Animation/
│       │   │   ├── POIAnimInstance.cs    # Game-specific anim controller
│       │   │   ├── States/               # Standing, InAir states
│       │   │   └── BlendSpaces/          # 1D/2D blend trees
│       │   ├── Input/                    # POIInputManager, command layer
│       │   ├── Damagable/                # Health, hit response
│       │   └── Spottable/                # PlayerSpot marker
│       ├── Prefabs/
│       │   ├── Enemy/                    # FlyingPuncher, MeleePuncher
│       │   ├── Projectiles/              # Projectile prefabs
│       │   └── Player/                   # Player prefab
│       ├── Scenes/
│       │   └── SampleScene.unity         # Main game scene
│       └── PathOfTheInfected.asmdef
│
├── PathOfTheInfected - Gameplay/         # Gameplay managers
│   └── PathOfTheInfected - Gameplay.asmdef
│
└── PathOfTheInfected - UI/               # UI implementation
    ├── Scripts/
    │   ├── Router/                       # POIRouter
    │   └── PlayerUI/                     # HealthBar, screens
    └── PathOfTheInfected - UI.asmdef
```

---

## 🏗️ Architecture Patterns

### Design Patterns Used

| Pattern | Usage |
|---------|-------|
| **Singleton** | `PlayerSm.Instance`, `POIAnimInstance.Instance`, `TidiGameplayMessagingSubsystem.Instance`, `TidiGCManager` |
| **State Machine** | Player movement (~16 states), Enemy AI (3-state), Animation (Standing/InAir), Boss phases |
| **SubSystem Pattern** | Combat subsystems (RBC, Combo, Perks) registered and updated centrally |
| **Observer Pattern** | Gameplay messaging (pub/sub), animation events (`OnAnimationEnded`) |
| **Command Pattern** | Input abstraction layer (`POIInputCommandManager`) |
| **Strategy Pattern** | Attack definitions as ScriptableObjects (`PlayerAttackSoBase`) |
| **Object Pooling** | Projectiles, particles, prefabs |
| **Component Pattern** | Modular UI (`UIComponent`, `ControllerIconDecorator`) |
| **BitFlags** | `CombatIntentFlags`, `CombatFlags` for packed state storage |

### Data-Driven Design

**ScriptableObjects** are used for:
- **Attack Definitions** — Damage, range, startup/active/recovery times
- **Enemy Stats** — HP, speed, aggro range
- **State Definitions** — Enemy states as SO assets (`EnemyWanderSO`, `EnemyAggroSO`)
- **Perk Data** — Passive bonuses (`PlayerPerkData`) - **Obsolete** (perks were removed late in development but the data structure remains)

---

## 🎓 About

This project was developed as part of a **Game Development class**. It demonstrates:

- **State machine architecture** for movement, animation, and AI
- **Physics-based character effects** (Verlet cloth/scarf simulation)
- **Combat systems** with combo tracking, damage multipliers, and perk integration
- **Decoupled architecture** using a typed messaging bus
- **Enemy AI** with spotting, pursuit, and attack behaviors
- **Boss battles** with phase transitions
- **Garbage collection management** for consistent performance
- **Modular UI framework** built on Unity UI Toolkit
- **Reusable component libraries** designed for cross-project use

---

## 🚀 Getting Started

### Prerequisites

- **Unity 6000.3.6f1** (Unity 6) or compatible 2022.x version
- **Windows** build support
- **New Input System** package (enabled in Project Settings)

### Installation

1. Clone or download this repository
2. Open **Unity Hub** → **Add** → Select the `PathOfTheInfected` folder
3. Wait for package restoration and assembly compilation
4. Open `Assets/PathOfTheInfected - Gameplay/Levels/MainMenu.unity`
5. Press **Play** in the Unity Editor

### Controls (Keyboard) ⌨️

| Action              | Input              |
|---------------------|--------------------|
| Move                | WASD or Arrow Keys |
| Jump                | Space or Z         |
| Dash                | C                  |
| Punch               | X                  |
| Return To Main Menu | Escape             |

### Controls (Controller) 🎮

| Action              | Input              |
|---------------------|--------------------|
| Move                | DPAD or Left Stick |
| Jump                | A                  |
| Dash                | RT                 |
| Punch               | X                  |
| Return To Main Menu | ☰                  | 

---

## 📝 Development Workflow

- **branching:** Feature branches follow `Feat_<FeatureName>` convention
  - Example: `Feat_ModularUIFramework`, `Feat_ExtendingAttacksAndWeaponSystem`
- **Main branch:** `main`
- **Development environment:** Unity Editor (no CLI build scripts)

---

## 📚 Documentation

| File | Purpose |
|------|---------|
| `README.md` | This file — feature list, tech stack, project structure |

---

**Built with Unity by a high school Game Development student - Harel Tidhar** 🎮
