UI Architecture Design (Unity – Modular Router/Screen/View System)
1. Overview
   This architecture defines a modular UI system built on top of Unity UI Toolkit.
   It separates responsibilities into four core layers:

UI Router (MonoBehaviour entry point)
UI Screen (logic + message handling layer)
UI View (visual representation layer)
Message Bus (decoupled communication layer)

The goal is:

Fully decoupled UI systems
No direct dependency between UI and gameplay logic
Easy replacement of UI frameworks
Highly reusable architecture across projects


2. Core Principles
   2.1 Separation of Concerns
   Each layer has a strict responsibility:

Router → wiring & lifecycle bridging
Screen → logic & state decisions
View → rendering only
Message Bus → communication only


2.2 Single Source of Truth
Gameplay systems own their own state.
UI does NOT store or duplicate gameplay state.
UI only:

requests state (initialization)
listens for updates (runtime)


2.3 Event-Driven Communication
All communication between systems is done via messages:

No direct references between gameplay and UI
No polling systems
No shared static state


3. System Architecture
   Unity Scene
   ↓
   UI Router (MonoBehaviour)
   ↓
   UI Screens (C# logic layer)
   ↓
   UI Views (UI Toolkit wrappers)
   ↓
   Visual Elements (actual rendering)


4. UI Router (MonoBehaviour)
   4.1 Responsibilities
   The UI Router is the entry point of the entire UI system.
   It is responsible for:

Receiving Unity lifecycle callbacks
Forwarding lifecycle events to screens
Creating and wiring screens and views
Managing initialization order


4.2 Lifecycle Methods
RouteAwake()
RouteStart()
RouteUpdate()
RouteFixedUpdate()

These mirror Unity lifecycle methods but exist only to forward execution.

4.3 Responsibilities Summary

Instantiate UI Screens
Create UI Views
Inject dependencies into Screens
Maintain execution order
No UI logic allowed


5. UI Screen (Logic Layer)
   5.1 Responsibilities
   UI Screens act as the controller layer of the UI system.
   They are responsible for:

Receiving messages from the message bus
Handling UI logic decisions
Updating UI Views
Managing screen-specific state


5.2 Lifecycle Methods
Screens use a simplified lifecycle:
Initialize()
Tick()
FixedTick()
Dispose()


5.3 Behavior Rules

Screens do NOT access raw UI Toolkit elements
Screens do NOT contain rendering logic
Screens ONLY call methods on Views
Screens ONLY react to messages or initialization flow


5.4 Example Responsibilities


HealthBarScreen

listens to HealthChanged messages
updates HealthView



MainMenuScreen

listens for navigation events
updates button states




6. UI View (Presentation Layer)
   6.1 Responsibilities
   UI Views are pure rendering wrappers around UI Toolkit.
   They are responsible for:

Holding references to VisualElements
Exposing safe UI methods
Rendering state provided by Screens


6.2 Rules

No game logic
No message subscriptions
No decisions
No state ownership


6.3 Example API
SetHealth(int value)
SetMaxHealth(int value)
Show()
Hide()
SetVisible(bool value)


6.4 Internal Structure
Views may cache UI elements:
Label healthText;
VisualElement root;

But must NOT contain logic beyond presentation.

7. Message Bus System
   7.1 Purpose
   The Message Bus enables complete decoupling between:

Gameplay systems
UI screens
UI initialization flow


7.2 Message Types
State Change Messages

HealthChanged
StaminaChanged
AmmoChanged

Initialization Messages

UIReady
RequestStateSnapshot
StateSnapshotResponse


7.3 Communication Flow
Runtime updates
PlayerHealth → Message Bus → UI Screen → UI View

Initialization flow
UI Screen → RequestStateSnapshot
PlayerHealth → ResponseSnapshot
UI Screen → Initialize View


8. Initialization Pattern
   Each system follows this pattern:
   Step 1: UI signals readiness
   UI Screen → UIReadyMessage

Step 2: Gameplay system responds
PlayerHealth → SendCurrentState

Step 3: UI initializes
Screen → View.SetState()

Step 4: Subscribe to updates
Screen → Subscribe to HealthChanged


9. Dependency Flow Rules
   Allowed dependencies

Router → Screens, Views
Screen → Views, Message Bus
View → UI Toolkit only


Forbidden dependencies

View → Screen
View → Game logic
Screen → Unity UI tree directly
Gameplay → UI Screens


10. Design Benefits
    10.1 Modularity
    UI components can be swapped or replaced without affecting gameplay systems.

10.2 Framework Flexibility
UI Toolkit can be replaced with:

Nova
IMGUI
Custom UI systems

without changing controller logic.

10.3 Reusability
Screens and Views can be reused across projects.

10.4 Testability
Screens can be tested without Unity runtime.

11. Summary Model
    Router (wiring + lifecycle)
    ↓
    Screen (logic + message handling)
    ↓
    View (rendering only)
    ↓
    UI Toolkit (visual output)

Communication:
Message Bus = only communication layer
No direct references allowed between systems


12. Final Architecture Rule
    If uncertain about responsibility:

If it decides → Screen
If it shows → View
If it wires → Router
If it communicates → Message Bus

