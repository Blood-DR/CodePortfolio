# Code Portfolio

C# projects built while teaching myself game development. My focus is gameplay programming and game/level design.

| Project | What it is | Tech |
|---|---|---|
| [**SAM Command**](SAM_Command) | First-person radar-operator defence game (featured) | C#, s&box (Source 2), Razor UI |
| [NotesAPI](NotesAPI) | REST API for notes | ASP.NET Core minimal API, SQLite |
| [Personal_Notes_Manager](Personal_Notes_Manager) | Desktop notes app | WinForms, SQLite |

---

## Featured: SAM Command

A first-person air-defence game built in **s&box** (Facepunch's C# engine on Source 2). You work a bunker's radar and fire-control stations, defending cities from incoming aircraft with a limited number of SAM missiles.

> **Note:** this folder holds the game's source code and UI files only. The engine project (scenes, models, prefabs) is no longer available, so the game can't be run from this repository. [Add a link to any surviving video or screenshots here.]

### The core loop

1. **Spot** – a rotating radar sweep paints contacts on an in-world radar panel. Blips fade between sweeps, so you only ever see where a plane *was*.
2. **Identify** – click a blip to select it and read its bearing and range. Contact types (Hostile, Friendly, Spoofer, Decoy) are defined so that shooting the wrong thing can be a mistake.
3. **Generate a fire solution** – each aircraft carries a fire-control code (e.g. `B2-G4`). You enter the correct code at the terminal to fire; a wrong code is a guaranteed miss and still costs ammo.
4. **Fire** – a SAM site with ammo, in range and inside its firing arc launches a homing missile.
5. **Adapt** – from later days, aircraft can **manoeuvre**: they swap to a new target city and a new code, invalidating your solution mid-engagement.

Each in-game day runs **Warm-up → Wave → Debrief → Save**. A campaign runs days 1–13 and then unlocks Endless mode.

### Design decisions

- **Pressure comes from throughput, not speed.** In Endless mode aircraft speed stays roughly constant, so reaction time per engagement is fair. Difficulty scales with the *number* of contacts, escort formations and tighter spacing between groups.
- **Information is deliberately stale.** Radar sweep and blip decay mean you act on old data, and the manoeuvre mechanic punishes locking in too early.
- **Sites you haven't unlocked yet** act as allied NPC defenders that fire on their own, which makes the world feel alive before you take control.
- **Maneuver frequency ramps with the day** (0% on day 1, up to about 60% by day 11), giving a gentle learning curve.

### Code tour

| File | What it demonstrates |
|---|---|
| `Managers/GameManager.cs` | Day-phase state machine with events, campaign vs endless modes, save/continue, and a fix for a component init-order race (restoring state one frame after startup) |
| `Radar/RadarSystem.cs` | Sweep detection that handles angle wrap-around, blip decay, world-to-panel coordinate mapping, and fire-solution generation |
| `Radar/RadarInputHandler.cs` | Clicking a 3D in-world screen: ray trace, convert the hit to panel UV coordinates, then pick the nearest blip, site or city |
| `Enemies/Aircraft.cs` | Movement, leaking onto a city, random manoeuvres and code regeneration |
| `Weapons/SAMSite.cs`, `Missile.cs` | Ammo, firing-arc and range checks (angle between site forward and target), NPC autopilot, homing missiles with a lifetime failsafe |
| `Waves/WaveSpawner.cs`, `CampaignWaveConfig.cs` | Hand-authored campaign waves per day, plus a procedural endless generator with an escalating difficulty curve |
| `Save/` | JSON save system with versioning, which keeps the Endless unlock when starting a new campaign |
| `Managers/StationManager.cs` | Entering and leaving station views while restoring player and camera state |
| `UI/` and `MainMenu/` | Razor/SCSS interfaces: terminal, contact info panel, radar panel, escape menu, main menu |
| `UI/cctvDisplay.cs` | CCTV monitors using render-target textures and per-monitor material copies |

### Known issues and what I'd improve

- A campaign day with no wave config returns zero entries, so the wave never ends. It needs a guard that finishes the wave when there is nothing to spawn.
- Day history is only written when the campaign finishes, not for every day.
- Saved cities and sites are matched by label/name, so renaming objects breaks saves. Stable IDs would be safer.
- `StationManager` finds the look controller's angles by reflection, which is brittle. Exposing a proper API on the controller would be better.
- `DayPhase.Save` is skipped through immediately, and the tutorial is a placeholder.
- Static events (`Aircraft.OnResolved`, `City.OnDamaged`) need every subscriber to unsubscribe cleanly on scene change.

---

## NotesAPI

A small REST API for managing notes: create, read, update and delete over HTTP, stored in SQLite.

- ASP.NET Core minimal API with a repository class for data access
- Parameterised SQL queries
- Try it with the requests in `NotesAPI.http`

## Personal_Notes_Manager

A Windows desktop notes app. Create, edit and delete notes in a list, stored in a local SQLite database.

- WinForms UI with SQLite persistence
- Delete confirmation and selection preserved after saving

---

## About me

Self-taught, building a game dev portfolio in C#. Next up: a small, playable third-person combat and level-design prototype in Unity.

Contact: [your email / links]
