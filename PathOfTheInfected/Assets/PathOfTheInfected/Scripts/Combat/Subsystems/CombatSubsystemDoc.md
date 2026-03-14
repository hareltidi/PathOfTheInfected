```mermaid
%% Combat Subsystem Architecture Diagram
%% Shows PlayerCombat owning subsystems and data flow from hits

classDiagram
    class PlayerCombat {
        +CurrentAttack : PlayerAttackSoBase
        +CombatFlags : BitFlags
        +CombatIntentFlags : BitFlags
        +Subsystems : List~CombatSubsystem~
        +RegisterHit(hitResult)
        +Update(deltaTime)
    }

    class CombatHitContext {
        +Source : GameObject
        +Target : GameObject
        +AttackDefinition : PlayerAttackDefinition
        +ComboDamageScaling : float
        +AttackerState : enum/flags
        +OptionalFlags : Dictionary/Bitmask
    }

    class CombatSubsystem {
        +Owner : PlayerCombat
        +IsInDebugMode : bool
        +Initialize(owner, debug)
        +Update(deltaTime)
        +FixedUpdate(fixedDeltaTime)
        +RegisterHit(context)
        +OnRegisterHit(context)
        +ClearState()
    }

    class ComboSubsystem {
        +comboCount : int
        +comboTimer : float
    }

    class RBCSubsystem {
        +currentResets : int
        +timers / cooldowns
    }

    PlayerCombat --> "1..*" CombatSubsystem : owns
    CombatSubsystem <-- CombatHitContext : receives context
    CombatSubsystem <|-- ComboSubsystem
    CombatSubsystem <|-- RBCSubsystem

```
### Detailed Explanation of the Combat Subsystem Architecture

The combat subsystem architecture is designed to provide a **modular, extensible, and robust framework** for handling player and enemy combat interactions in a precise, skill-based platformer environment. At its core, the architecture separates **attacker feedback**, **combat mechanics**, and **reactive subsystems**, ensuring a clear distinction of responsibilities and maximum flexibility for future features.

1. **PlayerCombat as the Owner**

   `PlayerCombat` is the central orchestrator of combat mechanics. It manages:
    - The **current attack** (`CurrentAttack`), representing the active `PlayerAttackSoBase` instance.
    - A collection of **combat subsystems**, such as combo tracking, RBC (Repeated Blocking/Counter) logic, and any future reactive mechanics.
    - Feeding confirmed hits into subsystems via a structured **CombatHitContext**, ensuring that all subsystems receive consistent, clean data about successful attacks.

2. **Combat Subsystems**

   Each combat subsystem inherits from the base `CombatSubsystem` class. Subsystems are **modular components** that encapsulate specific reactive behaviors, such as:
    - Combo progression
    - Enemy state reactions (stagger, damage animations)
    - Player state modifiers (temporary buffs or cooldowns)

   Subsystems have:
    - A reference to their owner (`PlayerCombat`) to access relevant combat state.
    - Debug logging capability (`IsInDebugMode`) to visualize and verify subsystem behavior.
    - `Update(float deltaTime)` and `FixedUpdate(float fixedDeltaTime)`  methods for handling timers, cooldowns, or time-sensitive logic.
    - `RegisterHit(CombatHitContext context)` and optional `OnRegisterHit` hooks for processing confirmed combat hits.

   The design ensures that **subsystems are independent and stackable**: multiple subsystems can react to the same hit without conflicts, and each manages its own internal state.

3. **CombatHitContext**

   `CombatHitContext` is a dedicated data structure that contains all relevant information about a confirmed hit:
    - Source and target entities
    - Attack definition
    - Position, velocity, or other attack-specific parameters
    - Any gameplay modifiers relevant to subsystems (e.g., combo damage scaling, in-air flag)

   By feeding **subsystems this context** instead of raw `HitResult` objects, we preserve the **single responsibility principle**: `HitResult` remains focused solely on communicating the outcome to the attacker, while `CombatHitContext` provides rich data for subsystem processing.

4. **HitResult vs Subsystem Context**

    - `HitResult`: Focused on the **attacker’s feedback** — did the attack deal damage, miss, or get blocked? It also stores the final calculated damage.
    - `CombatHitContext`: Focused on **subsystem needs** — detailed information for combos, RBC, or other reactive systems. Constructed **after confirming a valid hit**.

   This separation ensures that the **subsystems only react to confirmed hits**, maintaining clean architecture and reducing the risk of misfires or incorrect state updates.

5. **Flow of Combat Logic**

    1. Player executes an attack (`ActivateAttack`) through `PlayerCombat`.
    2. Attack hitbox (e.g., `PlayerPunchHitBox`) performs collision checks.
    3. For each valid target hit:
        - `HitData` is built and processed.
        - `HitResult` is returned to the attacker to update feedback (animations, sound, hit effects).
        - If the outcome is `Damaged`, `CombatHitContext` is constructed and passed to all registered subsystems via `RegisterHit`.
    4. Subsystems update their internal logic in `Update(deltaTime)` and `FixedUpdate(fixedDeltaTime)` and react according to the context (combo chaining, RBC timing, counters, etc.).
    5. PlayerCombat continues to manage global timers, states, and future subsystem registration.

6. **Key Advantages**
    - **Clear Separation of Responsibilities:** Attacker feedback (`HitResult`) vs subsystem reactions (`CombatHitContext`)
    - **Modularity:** New subsystems can be added without modifying existing systems.
    - **Extensibility:** Future mechanics like stagger chains, elemental effects, or buffs can leverage the same architecture.
    - **Time-Sensitive Management:** Each subsystem can implement its own timers or cooldowns without interfering with others.
    - **Debug-Friendly:** Subsystems can log detailed hit context information without polluting core gameplay logic.





