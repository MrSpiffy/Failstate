# Development

## Choose the Correct Branch

This guide targets the committed city and relay prototype on `codex/failstate-cleanup-layout`. Use an explicit branch clone to reproduce that snapshot:

```sh
git clone --branch codex/failstate-cleanup-layout https://github.com/MrSpiffy/Failstate.git
cd Failstate
```

Preserve local work before switching branches in an existing checkout. [Status](STATUS.md) documents which later features have not been published yet.

## Editor Setup

1. Install Unity **6000.3.12f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
2. Install Git and make sure it is available to Unity. The package manifest includes a Git dependency for the MCP Unity editor integration.
3. Open the repository through Unity Hub. Let Unity restore packages and compile scripts.
4. Open `Assets/Scenes/MainScene.unity` explicitly and press Play.

The package manifest and lock file are versioned. Unity's generated `Library`, IDE project/solution files, logs, build output, and user settings are not source inputs. Unity regenerates IDE solutions when opening the project or using **Preferences > External Tools > Regenerate project files**.

The MCP Unity package supports AI/editor tooling; it is not a gameplay feature and a running MCP client is not required to play the scene. The project also includes Unity UI, TextMesh Pro assets, and Input System packages. Gameplay currently uses legacy `UnityEngine.Input` calls, so preserve the project's input handling settings when diagnosing input issues.

## Builds and Scene Reloads

The development branch includes `Assets/Scenes/MainScene.unity` as an enabled build scene. In **File > Build Profiles**, verify it remains in the active profile's scene list before building, particularly if a custom profile overrides the shared list. Restart code reloads the active scene by build index. Unity's product name is `Failstate`; the historical `LastMaintenance` label has been replaced without changing gameplay logic.

Install the appropriate platform build-support module through Unity Hub. No downloadable player release or licensed Unity CI build is currently configured. This repository cleanup does not establish standalone build support or measured hardware requirements.

## Scene Generation Tools

The published development branch provides these menus:

- **Tools > Failstate > Clean Main Scene Layout**
- **Tools > Failstate > Regenerate City Blockout**
- **Tools > Failstate > Regenerate Upper City Layer**

These tools modify scene content and may produce large serialized diffs. Start from a saved/checkpointed scene, regenerate intentionally, inspect placement and collision, and review the diff before committing. Routine playtesting does not require regeneration.

## Developer Console

Press `/` during gameplay. Run `help` for the commands available in that checkout; the command set grows with development. Useful inspection commands on the published starter-area branch include:

| Command | Purpose |
| --- | --- |
| `help` | Show command syntax |
| `netstatus` | Inspect infrastructure state |
| `restore` | Restore robot integrity |
| `add metalscrap 10` | Add resources for a targeted test |
| `creative on` / `creative off` | Toggle creative movement/decay behavior |
| `revealmap` | Reveal map for layout inspection |
| `mapdebug on` / `mapdebug off` | Toggle debug map presentation |
| `scan` | Trigger scan behavior |

Use a fresh session without cheats for economy and pacing tests. Console-assisted setup is useful for isolating defects, but does not establish normal progression or balance.

## Verification

Open the scene, check Unity's Console, and use the [playtest protocol](PLAYTESTING.md) for gameplay changes. This project does not currently include a project-specific automated regression suite; the presence of Unity test packages is not evidence of passing gameplay tests.

After Unity generates the C# projects, `dotnet build Assembly-CSharp.csproj` and then `dotnet build Assembly-CSharp-Editor.csproj` can catch compilation issues. Run them sequentially. They are optional compile checks, not substitutes for Unity import, player builds, collision checks, or playtesting.

Documentation links can be checked without Unity using PowerShell:

```powershell
./Tools/Test-Documentation.ps1
```

This checks local Markdown file targets and required repository documents; it does not validate external websites or gameplay behavior.
