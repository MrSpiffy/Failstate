# Gameplay Capture Guide

No project gameplay screenshots or GIFs are currently tracked. The images under TextMesh Pro are vendor examples, not Failstate captures. The README contains an HTML comment marking the intended visual location; it deliberately renders no empty image boxes.

Capture the committed version you plan to submit. The current working scene contains unpublished experiments, so use a clean clone of `main` when preparing submission evidence. Do not present concept art as implemented gameplay.

## Recommended Captures

Place two to four actual captures in `Documentation/Images/`. A PNG is sufficient; a short GIF is optional. Keep the main image readable around 1280-1600 pixels wide and keep files reasonably small; avoid large raw video files in Git.

| Filename | What to Capture | What It Shows |
| --- | --- | --- |
| `generated-city.png` | Third-person street view with the player visible, several building blocks, a plaza or route junction, and overhead-city structure. Use a naturally reachable location. | The actual generated environment, player perspective, and current blockout quality. Best lead image. |
| `infrastructure-map.png` | Enlarged map showing the workshop, relay sites, and explored/restored cells during a real run. | Spatial organization and the relationship between city generation and progression. If using `mapdebug`, label the image as a developer map view. |
| `relay-restoration.gif` or `relay-restoration.png` | A relay before/after restoration from the same player position, or a short clip of the final activation and visible feedback. | Infrastructure state changing in the implemented prototype. Keep the HUD visible if it helps establish what changed. |
| `workshop-survival.png` | Player at the workshop with inventory/crafting or the system-integrity HUD legible. | The survival/resource loop and a concrete interaction beyond environment generation. |

Start with the first two. Use the others only if they add distinct information. Note the commit and any developer commands used in image captions or this document. Ordinary play screenshots should not be staged with features unavailable in the submitted version.

## Adding the Images

Replace the README's capture comment after the files exist. For the first two images:

```markdown
![Third-person view of Failstate's generated starter city](Documentation/Images/generated-city.png)

![Explored cells and relay sites on the infrastructure map](Documentation/Images/infrastructure-map.png)
```

Use accurate captions such as "Starter-area blockout" or "Developer map view." Run `./Tools/Test-Documentation.ps1` afterward and commit the images with the README edit. Do not add image links before their target files are present.
