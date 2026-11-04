# Decisions

Append-only, newest at the bottom. Old entries are not edited. A new entry supersedes them.

## 2026-11-04: Project bootstrap
**Decision:** Build Ashbound, a 2D side-scrolling action roguelite, as a public portfolio project for
WebGL, with the stack in `CHARTER.md`.
**Alternatives considered:** The free 2D Roguelike sample from Unity is turn-based and tile-based, so
it does not fit real-time action. Corgi Engine would speed up movement, but its code is third-party
and could not be published in a public repository.
**Why:** Writing the movement and combat code is the part worth showing in a portfolio, and a public
repository needs everything in it to be redistributable.
**Consequences:** Art is supplied separately and must have a license that allows publishing. M0 is a
gray-box of the core loop.

## 2026-11-04: Repository layout
**Decision:** The repository root is the Unity project. Project documents live in `design/`, and
`docs/` is reserved for the WebGL build served by GitHub Pages.
**Why:** Pages can only serve the root or `docs/`, and a build must not overwrite design documents.
**Consequences:** Run the build from `Ashbound > Build WebGL`. Committing a build adds its size to the
history, so builds are committed only at milestones.

## 2026-11-04: Assemblies
**Decision:** `Ashbound.Logic` holds game rules and has no engine references. `Ashbound.Core` holds
bootstrap and scene flow. `Ashbound.Editor` holds build and import tools. `Ashbound.Gameplay` and
`Ashbound.UI` are created when their first scripts exist.
**Why:** Rules such as level assembly and loot run in fast EditMode tests without the engine.

## 2026-11-04: Deterministic random
**Decision:** All run content uses `SeededRandom` (SplitMix64) with one forked stream per subsystem.
**Why:** A seed must reproduce a run on every platform, and loot rolls must not shift the level layout.

## 2026-11-04: Pixel scale
**Decision:** Sprites import at 16 pixels per unit with point filtering, on a 320x180 reference view.
**Why:** It matches 16-pixel tiles and scales cleanly to 1280x720. To be revisited if the art uses
another resolution.
