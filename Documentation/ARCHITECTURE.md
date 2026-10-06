# Architecture and Source Tour

This guide describes the published starter-area branch, [`codex/failstate-cleanup-layout`](https://github.com/MrSpiffy/Failstate/tree/codex/failstate-cleanup-layout). Additional workspace experiments are listed separately in [Status](STATUS.md). `main` still contains the earlier survival foundation.

## System Responsibilities

Project-specific gameplay scripts currently live together in `Assets/Scripts`; editor tools live in `Assets/Editor`. The table is a reading guide, not a claim that the code is already divided into independent packages.

| Area | Entry points | Responsibility |
| --- | --- | --- |
| Lower-city generation | `CityBlockoutGenerator`, `LocalGridLandmarkSite` | Template-guided starter layout, street hierarchy, plazas, district membership, walkable cells, building/floor blockout |
| Overhead city | `UpperCityLayerGenerator` | Upper-city masses, support structures, openings, and scale cues |
| World population | `GeneratedWorldSpawner`, `ScrapPickup`, `RelaySalvageCache`, `HazardZone` | Resources, relay-site supplies, hazards, discoveries, and landmark placement |
| Workshop | `BaseCampLayoutManager`, `BaseCampZone`, storage/terminal stations, `RechargeStation` | Refuge, root relay, storage, crafting access, recharge, and archive interactions |
| Infrastructure | `InfrastructureNetworkManager`, `InfrastructureNode`, `RelayRestorationController` | Relay registration, prerequisites, restoration steps, network state, and milestone feedback |
| Survival and movement | `PlayerCondition`, `PlayerMovement`, `CameraFollow`, `SunlightZone` | Integrity decay, failure, movement effects, camera effects, and environmental recovery |
| Inventory and crafting | `PlayerInventory`, `ItemDatabase`, `CraftingRecipeDatabase`, `ItemUseSystem` | Item counts, recipe costs, spending, outputs, and consumable effects |
| Navigation | `MinimapUI`, `ScanPulseEffect`, `RestorationDistrictVisual` | Explored cells, map markers, scan feedback, and district restoration presentation |
| Objectives and memory | `FirstRunObjectiveManager`, `EnvironmentalFragment`, `NetworkObjectiveUI` | First-run milestones, recovered traces, and objective wording |
| UI coordination | `GameReferences`, `UIStateManager`, `InteractionPromptUI`, individual UI components | Scene references, input/cursor state, interface ownership, and interaction prompts |
| Iteration tools | `DevConsoleUI`, `DevConsoleCommandSystem`, `FailstateSceneTools` | Targeted debugging, map inspection, scene cleanup, and regeneration |

Browse the [published scripts](https://github.com/MrSpiffy/Failstate/tree/codex/failstate-cleanup-layout/Assets/Scripts) and [editor tools](https://github.com/MrSpiffy/Failstate/tree/codex/failstate-cleanup-layout/Assets/Editor).

## Spatial Data Flow

The generator supplies the cell coordinate system and records walkability, street/plaza classifications, and restoration districts. World population and navigation consume these records instead of each inferring the city from rendered meshes. The saved cell records also let dependent systems find a generated layout after editor regeneration and scene serialization.

Template-guided generation gives the workshop, three relay sectors, and progression routes a deliberate relationship while retaining generated local detail. This is a constrained starter-area layout, not an infinite city simulation. The upper-city layer is primarily environmental structure rather than a second fully playable world.

## Progression Flow

The player reconnects the Base Camp root, then restores Signal, Power, and Transit. Each relay exposes diagnostic, component-installation, and local activation steps. The network manager records restored nodes and prerequisites; the objective manager turns those states into the first-run objective sequence. Returning to the workshop after the chain completes produces the memory milestone.

Restoration state feeds world and map feedback. The deep-city signal marks an intended continuation beyond the starter area; it is not a finished second sector.

## Survival and UI Flow

Core, Mobility, and Perception are separate integrity values. Decay and hazards reduce integrity; recharge and consumables restore it. Movement and camera behavior respond to deterioration. Core depletion drives the failure state.

`UIStateManager` coordinates gameplay input, cursor lock, and pause/failure behavior. Inventory and workbench interfaces operate on shared inventory and recipe data. `GameReferences` provides scene references, with object lookups used as fallbacks in several components.

## Current Tradeoffs

- Generation and map scripts have grown large through iteration. Layout, mesh construction, gameplay placement, and presentation still share substantial ownership.
- Some generated geometry is serialized into the scene, so regeneration can create large scene diffs. Those diffs need deliberate review.
- Scene objects and initialization order remain important dependencies. This is not a dependency-injected or comprehensively unit-tested architecture.
- Input defaults and public component fields can be overridden by scene serialization; a code default alone does not establish the active playtest value.
- Progression and inventory are session state. There is no implemented disk-save system.

The current prototype is useful for inspecting systems integration and design iteration. Its runtime behavior and performance should be evaluated using [Playtesting](PLAYTESTING.md), rather than inferred from architecture alone.
