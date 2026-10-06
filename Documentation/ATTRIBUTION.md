# Authorship and Attribution

## Development

The developer directs the game's design, system requirements, and playtest-driven iteration. Recorded playtest feedback has shaped mobility/resource balance, relay benefits, workshop versus field crafting, and the intended service-route behavior. Those are concrete areas of design direction and evaluation, not a claim that every implementation was manually authored.

**ChatGPT/Codex assistance** has contributed to code, technical discussion, debugging, and documentation. Generated implementation has been integrated and revised as the prototype evolves. The project should not be presented as wholly handwritten implementation, and repository history alone does not establish who typed each line or which architectural details were personally invented.

Portfolio review can inspect the implemented systems, source history, design decisions, known issues, and the difference between intended behavior and verified results. The developer should be prepared to explain the code path from generation through placement and restoration, discuss the documented tradeoffs, and distinguish personal decisions from assisted implementation. This documentation does not assign a percentage of human versus generated code or claim work history not established by the project.

## Project Content

The current city and relay environment is primarily prototype geometry constructed through project scripts and Unity primitives. External concept art and inspiration images inform the intended atmosphere and scale; they are not evidence of rendered gameplay and are not redistributed as repository preview images in this pass.

## Included Third-Party Content

| Content | Location | Attribution / terms |
| --- | --- | --- |
| TextMesh Pro assets and examples | `Assets/TextMesh Pro` | Included Unity/TextMesh Pro package content; retain supplied notices and applicable package terms |
| Liberation Sans | `Assets/TextMesh Pro/Fonts` | SIL Open Font License 1.1; supplied `LiberationSans - OFL.txt` |
| Additional example fonts | `Assets/TextMesh Pro/Examples & Extras/Fonts` | Retain the individual OFL/license files supplied with each font; do not assume all example assets have the same terms |
| EmojiOne sample sprites | `Assets/TextMesh Pro/Sprites` | Supplied `EmojiOne Attribution.txt` refers to the creator and licensing terms; do not relabel as original or CC0 |
| Unity packages | `Packages/manifest.json`, `Packages/packages-lock.json` | Dependency versions and sources are recorded in the manifest/lock; each package retains its own terms |
| MCP Unity editor integration | Git dependency in `Packages/manifest.json` | [CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity); editor tooling, with upstream license terms |

This is a provenance guide, not a new license grant or an exhaustive legal audit. No blanket software license has been granted for original project code. Third-party licenses and notices continue to apply independently. Future imported art, sound, fonts, and generated assets should have their source and terms documented here before public redistribution.
