# Submission Review

Reviewed October 6, 2026 for a software-engineering internship portfolio. This is a repository/engineering review, not an assessment of Riot's hiring criteria. No company-specific technology, VALORANT integration, multiplayer, networking, or production-readiness claims have been added.

## Recommendation

**READY AFTER promoting the reviewed committed prototype, adding two real captures, and doing a clean-checkout smoke test.** A finished game is not required to show the current engineering work. The uncommitted gameplay experiments do not need to be included in this submission snapshot.

The short review path is README -> its six source entry points -> Architecture. The first screen identifies the game, engine/language, WIP state, and shared-spatial-data engineering focus. AI assistance is disclosed in Authorship and Attribution rather than the opening project summary.

## Branch Assessment

- The development branch contains the generated city and relay progression missing from the current `main` gameplay tree. It is the stronger source-review candidate.
- Before this pass, `main` and development had separate documentation commits based on the same older ancestor. An initial merge-tree simulation was clean.
- The final merge-tree simulation reports seven add/add conflicts: README, five documentation files, and the documentation-check script. Resolve those specific paths using the reviewed development versions; do not blindly select one side for all files. No gameplay-source conflicts were reported for the reviewed branch pair.
- The workspace still contains earlier uncommitted gameplay/scene changes and four new gameplay scripts with their metadata. They are excluded from this presentation commit and were not certified as a submission version. Avoid `git add .` or switching this dirty checkout to `main`.
- The committed gameplay source can be inspected and compiled separately from those experiments. Compilation is not evidence of a clean Unity import, a complete run, a standalone build, or performance.

No merge, default-branch change, history rewrite, or branch deletion was performed by this pass.

## Suggested Promotion Commands

Run from the existing Failstate checkout after the presentation commit is published. These commands create a separate checkout, preserving the current dirty workspace. Choose another unused folder name if `../Failstate-submission` already exists.

```powershell
git fetch origin
git worktree add --detach ../Failstate-submission origin/main
git -C ../Failstate-submission merge --no-ff --no-commit origin/codex/failstate-cleanup-layout
```

Expect the documented add/add conflicts. For this reviewed branch pair, take the development copy of only these documents:

```powershell
git -C ../Failstate-submission restore --source=origin/codex/failstate-cleanup-layout --staged --worktree -- README.md Documentation/ARCHITECTURE.md Documentation/ATTRIBUTION.md Documentation/DEVELOPMENT.md Documentation/PLAYTESTING.md Documentation/STATUS.md Tools/Test-Documentation.ps1
git -C ../Failstate-submission diff --name-only --diff-filter=U
git -C ../Failstate-submission diff --cached --stat
```

The unresolved-file command must print nothing. If other conflicts appear, stop and inspect them; the remote may have changed since this review. Do not run the restore command in the original dirty workspace.

Open the separate checkout in Unity 6000.3.12f1. Let packages restore; check the Console; play movement, pickup, inventory/crafting, root repair, relay interaction, map, pause, and failure/restart. A full starter-chain run provides stronger evidence of economy/progression than these smoke checks. Capture images from this same source snapshot using [Captures](CAPTURES.md).

Stage only the added captures and intentional README image edits in the separate checkout, if any. Review them and the merge before committing. Then:

```powershell
git -C ../Failstate-submission commit -m "Merge reviewed starter-area prototype for portfolio submission"
git -C ../Failstate-submission push origin HEAD:main
```

The push is an ordinary fast-forward update from the fetched `main`; it will reject concurrent remote changes. Do not add `--force`. The merge preserves both histories and existing branches. GitHub already uses `main` as default, so no default-branch setting change is required after this promotion. The original checkout and its uncommitted experiments remain untouched. The explicit development-branch source links remain valid after promotion.

## Hygiene Review

- Inventoried the tracked tree, first-party script responsibilities, docs, package manifest/lock, Unity settings, prefabs/materials, third-party sample content, and branch history. Vendor content was reviewed for provenance/tracking, not line-by-line for gameplay behavior.
- No Unity cache/build folders, generated IDE solutions, credential files, or root handoff documents are tracked. Existing ignore rules cover those files. Local handoffs are retained.
- No project gameplay screenshots/GIFs are tracked; vendor sample media were not reused as portfolio evidence.
- The largest historical blobs are generated `MainScene.unity` revisions (largest inspected approximately 12.62 MiB). This is serialization volume, not an accidental committed player build. No LFS migration or history rewrite was performed.
- Common credential-pattern checks found no matches in the inspected workspace text or across the 16 reachable commits examined before this presentation commit. This is a targeted scan, not a guarantee that every possible secret format has been detected.
- No TODO/FIXME/HACK/XXX markers were found in the checked project scripts/tools/docs. Diagnostic logging and console tools are intentional and were retained.
- `SampleScene`, TextMesh Pro examples, and vendor assets were retained. Their actual dependency/usage would need further checking before removal; they are not an application blocker.
- Gameplay scripts remain in one folder, and several components are large. The documentation explains responsibilities and tradeoffs; no cosmetic folder move or broad refactor was used to disguise that structure.
- Corrected the Unity product name and added `MainScene` to the shared build scene list. No gameplay source or scene content was changed by this pass.

## Verification Boundaries

The documentation checker validates local links, ignoring examples in fenced code and HTML comments. Source links and cited methods are checked against the committed development snapshot. Common secret patterns and tracked-file hygiene are checked separately.

Runtime and editor source compilation were checked in an isolated archive of the committed gameplay snapshot against the installed Unity/package assemblies. Temporary generated project references were redirected to those local assemblies, and an unavailable optional VS Code analyzer was omitted. Both compiled with zero warnings/errors. This is an isolated source compile, not an untouched clean-clone Unity build. The local generated IDE projects were not changed.

No fresh Unity import, standalone build, human playthrough, or hardware performance profile was performed during this presentation pass. Those limits should remain explicit when discussing the project.
