# Submission Review

Reviewed October 6, 2026 for a software-engineering internship portfolio. This is a repository/engineering review, not an assessment of Riot's hiring criteria. No company-specific technology, VALORANT integration, multiplayer, networking, or production-readiness claims have been added.

## Recommendation

**READY AFTER adding three real captures and doing a clean-checkout smoke test while capturing.** A finished game is not required to show the current engineering work. The uncommitted gameplay experiments do not need to be included in this submission snapshot.

The short review path is README -> its six source entry points -> Architecture. The first screen identifies the game, engine/language, WIP state, and shared-spatial-data engineering focus. AI assistance is disclosed in Authorship and Attribution rather than the opening project summary.

## Published Submission Snapshot

The reviewed starter-area prototype is published on `main`, the repository's default branch. The promotion preserves both existing histories through a merge commit. Its gameplay source and scene content match the previously reviewed committed prototype; only documentation and the previously reviewed setup settings accompany it.

Uncommitted gameplay/scene experiments and four new gameplay scripts with their metadata remain separate in the development workspace. They were not bundled into the submission snapshot. No branch was deleted, no history was rewritten, and the dirty gameplay workspace was not switched or reset.

A reviewer can clone normally, follow the README, and inspect source links without choosing a development branch. The remaining submission tasks are real captures from this version and a clean-checkout Unity smoke test. Compilation alone does not establish clean import, complete progression, a standalone build, or performance.

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

The documentation checker validates local links, ignoring examples in fenced code and HTML comments. Source links and cited methods are checked against the published submission snapshot. Common secret patterns and tracked-file hygiene are checked separately.

Runtime and editor source compilation were checked in an isolated archive of the committed gameplay snapshot against the installed Unity/package assemblies. Temporary generated project references were redirected to those local assemblies, and an unavailable optional VS Code analyzer was omitted. Both compiled with zero warnings/errors. This is an isolated source compile, not an untouched clean-clone Unity build. The local generated IDE projects were not changed.

No fresh Unity import, standalone build, human playthrough, or hardware performance profile was performed during this presentation pass. Those limits should remain explicit when discussing the project.
