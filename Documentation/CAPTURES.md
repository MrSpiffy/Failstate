# Gameplay Capture Guide

No project gameplay screenshots or GIFs are currently tracked. The images under TextMesh Pro are vendor examples, not Failstate captures. The README contains three HTML comments marking the intended visual locations; they deliberately render no empty image boxes. `Documentation/Images/.gitkeep` reserves the image directory without pretending a capture exists.

Capture the committed version you plan to submit. The current working scene contains unpublished experiments, so use a clean clone of `main` when preparing submission evidence. Do not present concept art as implemented gameplay.

## Recommended Captures

Place these three actual PNG captures in `Documentation/Images/`. Keep images readable around 1280-1600 pixels wide and keep files reasonably small; avoid large raw video files in Git. A GIF is not required.

| Filename | What to Capture | README Placement |
| --- | --- | --- |
| `generated-city.png` | A wide view showing several generated blocks, a plaza or street junction, and the overhead-city structure. Prefer a reachable location; if an editor or creative camera is needed, caption it as a developer overview rather than normal play. Show the actual blockout, not concept art. | Capture 1 comment: after the introduction, before Engineering Highlights. |
| `third-person-gameplay.png` | Normal third-person play at street level with the robot, surrounding environment, and Core/Mobility/Perception HUD visible. Choose a clear scene near a relay or scavenging location; close the console and menus. | Capture 2 comment: after Project Status, before Run from Source. |
| `infrastructure-map.png` | Enlarged map showing the workshop, relay sites, and explored/restored cells during a real run. This demonstrates the connection between generated spatial data and progression. If using `mapdebug` or `revealmap`, label it as a developer map view. | Capture 3 comment: after Engineering Highlights, before Where to Start. |

Note the captured commit and any developer commands used in image captions or this document. Ordinary play screenshots should not be staged with features unavailable in the submitted version. While capturing from the clean clone, confirm the scene imports, enters Play mode, and has no unexpected Console errors; this is the remaining setup smoke check, not a claim of full gameplay validation.

## Adding the Images

Replace each corresponding README comment only after its image file exists. Use these Markdown lines at the locations listed above, with an accurate short caption below each:

```markdown
![Wide view of Failstate's generated starter city](Documentation/Images/generated-city.png)

![Third-person gameplay with robot system-integrity HUD](Documentation/Images/third-person-gameplay.png)

![Explored cells and relay sites on the infrastructure map](Documentation/Images/infrastructure-map.png)
```

Use accurate captions such as "Starter-area blockout" or "Developer map view." Run `./Tools/Test-Documentation.ps1` afterward and commit/push the three images and README edit to `main`. Do not add image links before their target files are present.
