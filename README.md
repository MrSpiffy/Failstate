# Failstate

A 3D third-person survival/exploration game about a deteriorating maintenance robot restoring infrastructure in a dead industrial megacity.

**Unity 6 (6000.3.12f1) | C# | Single player | Active work in progress**

The starter-area prototype combines a generated city, resource scavenging, three failing robot systems, and a staged relay-restoration chain. Its main engineering focus is connecting procedural spatial data to gameplay: the city layout informs resource placement, navigation, hazards, and infrastructure progression.

<!-- Gameplay captures: insert real images here after following Documentation/CAPTURES.md.
Suggested lead image: Documentation/Images/generated-city.png
Suggested second image: Documentation/Images/infrastructure-map.png
Use images from the submitted committed version and label debug views. No concept art substitutes.
-->

## Engineering Highlights

- **Generation with gameplay constraints.** A template-guided layout establishes the workshop, relay sectors, and main routes; generated side streets, alleys, plazas, and connectivity repair fill in the local city. The generator preserves cell classifications and district ownership for downstream systems.
- **Shared spatial data across systems.** World spawning consumes walkable cells and semantic anchors, applies spacing and progression-placement rules, and records occupied cells. The map combines those records with exploration and restoration state instead of independently reconstructing the city from meshes.
- **Staged infrastructure progression.** Network prerequisites, relay diagnosis, component installation, and activation are represented separately. Restoration updates network state and feeds objectives, map reveal, environmental feedback, and the starter-chain memory milestone.
- **Interconnected survival and interaction.** Core, Mobility, and Perception affect failure, movement, and camera behavior. Inventory cost checks support crafting and repair; owner/priority-based prompts resolve competing nearby interactions.
- **Tools for iteration.** Scene regeneration, opening-pacing checks, and console commands make layout, economy, and progression easier to inspect. These are development tools, not a claim of comprehensive automated test coverage.

## Where to Start

These six entry points link to the published starter-area branch. Each leads to related components; the [architecture guide](Documentation/ARCHITECTURE.md) explains their data and control flow.

| Entry Point | What to Inspect |
| --- | --- |
| [CityBlockoutGenerator.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/CityBlockoutGenerator.cs) | `GenerateCityBlockout`, saved cell classifications, and connectivity repair show how a generated layout becomes a gameplay contract. |
| [GeneratedWorldSpawner.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/GeneratedWorldSpawner.cs) | `GenerateSpawnedObjects` shows constrained placement and ordered setup of starter supplies, infrastructure, hazards, and restoration feedback. |
| [InfrastructureNetworkManager.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/InfrastructureNetworkManager.cs) | `CanRestoreNode` and `GetNextRequiredNodeType` show network prerequisites; follow `RelayRestorationController` for per-relay repair steps. |
| [PlayerCondition.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/PlayerCondition.cs) | Integrity updates and movement/perception queries connect survival state to other gameplay components. |
| [PlayerInventory.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/PlayerInventory.cs) | Cost checks, spending, and `OnInventoryChanged` connect shared item state to crafting, repair, and UI refresh. |
| [DevConsoleCommandSystem.cs](https://github.com/MrSpiffy/Failstate/blob/codex/failstate-cleanup-layout/Assets/Scripts/DevConsoleCommandSystem.cs) | Command parsing and targeted state controls support reproduction and inspection of gameplay problems. |

## Current Scope

The playable source prototype includes the workshop, generated lower-city streets and relay plazas, an overhead-city blockout, scavenging/crafting, environmental hazards, map/scanning, data traces, and Base Camp -> Signal -> Power -> Transit progression.

Art and environmental presentation are blockout quality. There is no published player release, disk-save system, or finished second sector. Further scan, field-assembly, salvage-renewal, and service-corridor experiments remain local and are not included in the published gameplay snapshot. [Status and limitations](Documentation/STATUS.md) records that distinction and open playtest issues.

## Run from Source

For the committed starter-area snapshot described here, clone the development branch explicitly. [Status](Documentation/STATUS.md) records publication and branch details.

```sh
git clone --branch codex/failstate-cleanup-layout https://github.com/MrSpiffy/Failstate.git
```

Open the folder in Unity Hub with **Unity 6000.3.12f1**, allow package restore/compilation, then open `Assets/Scenes/MainScene.unity` and press Play. Git and network access are needed for the initial package restore. `MainScene` is included in this branch's build scene list. See [Development](Documentation/DEVELOPMENT.md) for build support, regeneration tools, and console commands.

**Controls:** WASD/arrows + mouse to move/look, Space to jump, E to interact/use, Tab for inventory, M for map, Q to scan, Escape to pause/close an interface. Map: wheel/arrows to zoom and left-drag to pan. `/` opens the developer console; R restarts after Core failure.

## Development Notes

- [Architecture and tradeoffs](Documentation/ARCHITECTURE.md)
- [Setup and debugging](Documentation/DEVELOPMENT.md)
- [Playtest protocol](Documentation/PLAYTESTING.md)
- [Current status and known limitations](Documentation/STATUS.md)
- [Authorship and asset attribution](Documentation/ATTRIBUTION.md)

## Authorship

I direct the game design, system requirements, and playtest-driven iteration. ChatGPT/Codex tools have assisted with implementation, debugging discussions, and documentation. This is **AI-assisted development**, not a claim that I hand-wrote every line. The implemented behavior, integration decisions, and remaining tradeoffs are documented for inspection; [Attribution](Documentation/ATTRIBUTION.md) gives further context and third-party notices.
