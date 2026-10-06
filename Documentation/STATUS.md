# Status and Limitations

Repository review: **October 6, 2026**. This page distinguishes published source from the newer local workspace. It is a snapshot, not a release announcement or test certificate.

## Availability

| Location | What is available |
| --- | --- |
| `main` | Earlier survival/inventory/crafting foundation, plus the repository presentation and documentation |
| [`codex/failstate-cleanup-layout`](https://github.com/MrSpiffy/Failstate/tree/codex/failstate-cleanup-layout) | Published starter-city and relay-progression prototype; gameplay snapshot `1e4ee61` |
| Local development workspace | Additional uncommitted relay-reward, resource-economy, field-assembly, scan, and service-corridor experiments |
| Releases | No published playable build at the time of this review |

## Published Starter-Area Prototype

The development branch includes template-guided city generation, workshop systems, survival decay, inventory/crafting, hazards, maps/scanning, a Base Camp root and three-relay chain, relay landmarks, data traces, a memory milestone, and an overhead city blockout.

This supports a starter-area gameplay loop. It does not establish a finished campaign, reliable balance across every generated layout, a seamless endless city, or a completed destination beyond the deep-city gate.

## Local Experiments Awaiting Publication

These features exist in the current workspace but must not be assumed present in a fresh GitHub clone:

- **Signal identity:** scan access tied to Signal restoration, with short-lived resource, hazard, and memory pulses rather than permanent exact loot markers.
- **Power identity:** broader survivability benefits, hazard reduction, and slow field-stabilization hubs.
- **Transit identity:** movement/strain benefits and direct service corridors linking the workshop and relay sites, including building carving and gate prototypes.
- **Field assembly:** basic repair kits and mobility/sensor patches assembled in the inventory; major relay components remain workshop recipes.
- **Resource resilience:** inefficient material conversion and intermittent autonomous salvage drops intended to reduce resource softlocks without making salvage unlimited at every location.
- **Presentation tuning:** removal of generated pickup debug labels and clearer relay-benefit/objective wording.

Human playtests reported that field assembly and renewable salvage reduced friction and avoided resource lockouts in those runs. Those reports are useful qualitative evidence, not proof that every resource configuration is safe.

## Known Problems and Open Questions

- **Service-corridor integration:** latest local playtests found gates standing free of building facades, enclosing walls interrupting existing streets, and map coverage missing buildings around routes outside the original starter mask. These remain open; this documentation pass does not fix them.
- **Travel and rewards:** some links save too little time, movement degradation can become frustrating, and restoring districts still needs stronger gameplay and environmental payoff.
- **Economy:** salvage-drop frequency and conversion costs need continued tuning against normal exploration and relay requirements.
- **Navigation:** scan readability, marker overlap, and the relationship between exploration reveal and restoration feedback need further testing.
- **World presentation:** prototype geometry, the visible city boundary, and limited distant-city treatment do not yet deliver the intended sense of scale.
- **Build setup:** no scene is included in the checked-in build profile. Add `MainScene` before player-build or restart testing.
- **Persistence and verification:** no disk-save system, project-specific automated gameplay suite, current standalone release, or measured performance targets are established here.

## Intended Direction

| Infrastructure | Intended player benefit |
| --- | --- |
| Base Camp | "I can survive": recharge, storage, crafting, refuge, and stabilization |
| Signal | "I can find things": useful but temporary indications of nearby opportunities |
| Power | "I can endure": reduced decay, safer travel, and useful powered infrastructure |
| Transit | "I can move": meaningful route shortcuts and less movement strain |

Relay benefits should be understandable before restoration and felt afterward. The intended effects extend beyond a relay's immediate district. Exploration should preserve uncertainty and physical investigation.

Isolated vantage points, visible restored/unrestored city contrast, a distant skyline, robot modules, richer audiovisual feedback, and later-city content are future direction, not promised implemented features. Combat and enemies are not the current development focus.

## Evidence Boundaries

Earlier development handoffs recorded successful C# and Unity compilation at those points in time. They are not fresh verification of the latest dirty workspace. This repository presentation pass checks documentation and Git hygiene; it makes no new claim of a complete gameplay playthrough, player build, or performance profile.
