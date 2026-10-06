# Failstate

A third-person survival and exploration prototype about a deteriorating maintenance robot in a dead industrial megacity. Scavenge parts, keep failing systems alive, and reconnect the city's infrastructure to recover fragments of its history and your own identity.

**Unity 6 | C# | Single player | In development | AI-assisted development**

## The Game

The workshop is a refuge in a dense lower city beneath a vast overhead urban layer. Outside it, the robot's Core, Mobility, and Perception deteriorate. Resources must serve both immediate survival and the repair of infrastructure that makes future expeditions possible.

The central design question is how restoration can feel like rebuilding both the city and the player. A relay should solve a problem the player has experienced: finding useful salvage, enduring longer expeditions, or moving efficiently through familiar territory. The prototype is being refined through full starter-area playtests, especially around travel friction, resource scarcity, and the payoff of restoration.

## Project Status

**This is an early gameplay blockout, not a finished game or polished vertical slice.** There is no published playable release yet.

The published starter-area prototype lives on [`codex/failstate-cleanup-layout`](https://github.com/MrSpiffy/Failstate/tree/codex/failstate-cleanup-layout). `main` currently contains the earlier survival foundation plus this repository documentation. Use the development branch to inspect or run the generated city and relay chain.

That branch includes:

- A generated starter city with a workshop, relay plazas, main streets, side streets, and alleys.
- Three degrading robot systems, scavenging, inventory, crafting, recharge, and environmental hazards.
- A Base Camp root relay followed by Signal, Power, and Transit restoration objectives.
- A minimap and enlarged map, exploration reveal, scanning, and restored-district feedback.
- Recoverable data traces, a workshop archive, and a starter-chain memory milestone.
- An upper-city blockout, relay landmark geometry, scene-generation tools, and a developer console.

Further experiments with field assembly, renewable salvage, discovery pulses, and gated service corridors are present in the local development workspace but **are not included in the published branch yet**. [Status and known limitations](Documentation/STATUS.md) separates published functionality, local experiments, and future direction.

## Engineering Highlights

These links point to the published development branch so they can be reviewed directly.

- **Generated layouts with authored intent.** [CityBlockoutGenerator](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/CityBlockoutGenerator.cs) combines a template-guided macro layout with generated neighborhood detail. It exposes cell data for streets, plazas, districts, and walkability to other systems.
- **Gameplay built on shared spatial data.** [GeneratedWorldSpawner](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/GeneratedWorldSpawner.cs) places resources, hazards, and infrastructure using the generated city. [MinimapUI](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/MinimapUI.cs) uses the same cell coordinates for exploration and presentation.
- **Infrastructure progression.** [InfrastructureNetworkManager](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/InfrastructureNetworkManager.cs) tracks network state; [RelayRestorationController](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/RelayRestorationController.cs) handles diagnosis, installation, activation steps, and restoration feedback.
- **Interacting survival systems.** [PlayerCondition](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/PlayerCondition.cs), movement, camera behavior, hazards, and repair items turn deterioration into mechanical consequences rather than a single health bar.
- **Iteration tooling.** [FailstateSceneTools](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Editor/FailstateSceneTools.cs) supports scene cleanup and regeneration. Console commands support targeted inspection of integrity, resources, map state, and progression.

See the [architecture guide](Documentation/ARCHITECTURE.md) for system responsibilities and current tradeoffs.

## Run from Source

1. Install **Unity 6000.3.12f1** through Unity Hub, and install Git. Network access is needed for the first package restore.
2. Clone the published starter-area branch:

   ```sh
   git clone --branch codex/failstate-cleanup-layout https://github.com/MrSpiffy/Failstate.git
   ```

3. Add the cloned folder in Unity Hub and open it with that editor version. Allow package import and script compilation to finish.
4. Open `Assets/Scenes/MainScene.unity` and press Play.

The project currently has no scenes listed in Build Profiles. Add `MainScene` to the active profile's scene list before building or testing scene-reload behavior. Editor setup, package notes, and debugging instructions are in [Development](Documentation/DEVELOPMENT.md).

## Controls

Defaults for the published starter-area prototype:

| Input | Action |
| --- | --- |
| WASD / arrow keys | Move |
| Mouse | Look |
| Space | Jump |
| E | Interact / use selected inventory item |
| Tab | Inventory |
| M | Toggle enlarged map |
| Q | Scan pulse |
| Mouse wheel / up and down arrows in map | Map zoom |
| Left mouse drag in map | Map pan |
| Escape | Pause / close active interface |
| / | Developer console |

Scene settings can override defaults. The local Signal rework changes scan availability; see [Status](Documentation/STATUS.md).

## Documentation

- [Architecture and source tour](Documentation/ARCHITECTURE.md)
- [Development setup and debugging](Documentation/DEVELOPMENT.md)
- [Playtesting protocol](Documentation/PLAYTESTING.md)
- [Status, limitations, and development direction](Documentation/STATUS.md)
- [Authorship and third-party attribution](Documentation/ATTRIBUTION.md)

## Authorship

Failstate is an **AI-assisted development project**, developed through iterative design direction, implementation, and playtesting with ChatGPT/Codex assistance. It should not be represented as wholly handwritten implementation. The code and documented tradeoffs are available for review.

The current environment uses prototype geometry and included Unity/TextMesh Pro assets. Concept references are design references, not screenshots of implemented gameplay. No blanket software license has been granted for original project code; included third-party materials retain their own terms. See [Attribution](Documentation/ATTRIBUTION.md).
