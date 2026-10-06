# Playtesting

Record the branch/commit, whether there are uncommitted changes, Unity version, layout seed/settings, and whether any console commands were used. The published branch and local experiments differ; consult [Status](STATUS.md) before reporting a missing feature.

## Normal Starter-Area Run

Start a fresh session without resource grants, creative mode, or map reveal.

- [ ] Find the workshop and understand what safety/recharge it provides.
- [ ] Gather the root-repair materials and restore Base Camp.
- [ ] Reach Signal, understand its promised benefit, diagnose it, and complete restoration.
- [ ] Complete Power and Transit in order, recording which materials require searching or crafting.
- [ ] Return to the workshop and reach the starter-chain memory milestone.
- [ ] Inspect the deep-city gate/signal without treating it as a completed next sector.
- [ ] Open/close inventory, map, workbench, pause, and console; check cursor and movement recover correctly.
- [ ] Verify `MainScene` is enabled in the active build profile and test failure/restart.

## Feel and Motivation

Record concrete moments rather than only an overall rating:

- How much time is spent exploring unfamiliar space versus retracing a known route?
- Does low Mobility create a decision, or merely prolong an unavoidable walk?
- Can the player describe a relay's benefit before repairing it, and notice it afterward?
- Are pickups found through investigation and scanning, or through debugging labels/markers?
- Do resource shortages encourage exploration without forcing a trip to a later sector or idle waiting?
- Is returning to the workshop a useful decision rather than a requirement for every small repair?

For traversal comparisons, time the same start/end locations before and after Transit with comparable integrity and movement settings. Report the route and any blocked crossings. A shorter line on a map is not evidence of a faster usable route.

## Checks for Local Experiments

Use these only in a checkout that contains the newer systems:

- [ ] Scan is unavailable before Signal and becomes useful after restoration.
- [ ] Pulses indicate eligible resources, hazards, and memory traces; collected/removed targets stop producing echoes.
- [ ] Field assembly spends the expected resources and leaves major components tied to workshop crafting.
- [ ] Salvage drops support recovery from shortages without replacing exploration or accumulating indefinitely.
- [ ] Power benefits can be noticed away from its immediate plaza; recharge hubs do not permit effortless permanent survival.
- [ ] All six intended workshop/relay service links can be walked after Transit.
- [ ] Closed gates block access before Transit and are attached to plausible tunnel entrances.
- [ ] Existing streets remain passable where service corridors cross them.
- [ ] Corridors near the old starter boundary have floor, enclosure, and map coverage; the player cannot escape into empty space.

Known corridor failures are recorded in [Status](STATUS.md). A checklist entry is a test to perform, not a claim it currently passes.

## Reporting a Defect

Include reproduction steps, expected/actual behavior, location or map screenshot, progression state, inventory and integrity where relevant, console errors, and whether restarting reproduces it. Prefer the repository's bug or playtest issue templates.

Compile checks, targeted console-assisted tests, complete human runs, and performance measurements should be reported separately. Preserve failed observations as well as passing results.
