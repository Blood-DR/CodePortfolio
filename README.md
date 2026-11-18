# Code Portfolio

C# projects I've built while teaching myself game development and software engineering. I'm focused on gameplay programming and game/level design.

## Featured: SAM Command (s&box)

A surface-to-air missile defence game built in **s&box** (Facepunch's C# game engine on Source 2). You defend cities from incoming threats by managing SAM sites, ammo and limited resources across a series of days.

**This folder contains the exported gameplay code only.** The original engine project files are no longer available, so the game can't be run from this repository. [Add a link to any surviving video or screenshots here.]

### Game loop

Each in-game day runs through four phases:

1. **Warm-up** – a countdown before the wave starts.
2. **Wave** – incoming threats spawn and you defend.
3. **Debrief** – results for the day.
4. **Save** – progress is checkpointed.

Two modes:
- **Campaign** – a fixed run of days, ending by unlocking Endless.
- **Endless** – starts at a later day so wave generation is already ramped up.

### What the code demonstrates (`GameManager.cs`)

- **Phase state machine:** a `DayPhase` enum with an `OnPhaseChanged` event, so other systems (UI, spawners) can react to changes.
- **Save and continue system:** day, money, city HP and SAM site ammo/damage are saved at the start of each day and restored when continuing.
- **Component init-order handling:** cities and SAM sites reset their values in their own `OnStart`, so restoring state in `GameManager.OnStart` was being overwritten. I fixed this by deferring the restore and the checkpoint write to the first `OnUpdate`, once everything has initialised.
- **Mode-specific rules:** campaign and endless runs are kept separate so endless runs never overwrite campaign progress.
- **Per-day stats model:** `DayConfig` records kills, leakers, friendly damage and money for the debrief and history.

### Known issues and what I'd improve

- Day history is only written when the campaign finishes, not for each day.
- Saved objects are matched by name/label, so renaming breaks saves. A stable ID would be safer.
- `DayPhase.Save` is currently skipped through immediately.
- The tutorial is a placeholder.
- `Instance` should be set in `OnAwake` rather than `OnStart`.



## About me

Self-taught, currently building up a game dev portfolio in C#. Working towards a playable gameplay prototype in Unity.


