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

## 2026-11-04: Art storage
**Decision:** `Assets/_Project/Art` is a submodule pointing to a private repository. Asset Store art
is never committed to this public repository.
**Alternatives considered:** Ignoring the folder with no backup, or replacing all paid-store art.
**Why:** Asset Store licenses do not allow redistribution, and the art still needs version control.
**Consequences:** A clone of the public repository compiles but has no art. Published WebGL builds
contain the art as part of the game, which the licenses allow.

## 2026-11-04: Game defaults
**Decision:** The target is a complete slice: one biome, one boss, runs of 15 to 20 minutes. Weapons
differ by damage, reach, speed and effects while reusing the sword animations. Movement is run, jump,
a dash with invulnerability and a dash attack. Levels join hand-made rooms by seed. Death ends the
run but keeps a currency for permanent unlocks. The setting is a knight bound to a castle in ashes.
The signature mechanic is chosen at the end of M0. Desktop browsers, keyboard and gamepad, English.
**Why:** The player art has a single weapon set, and this scope fits one developer.
**Consequences:** Wall slide and edge grab are deferred, although the art supports them.

## 2026-11-04: Architecture defaults
**Decision:** The player is a kinematic Rigidbody2D driven by a state machine. Hitboxes are enabled
by Animation Events. Enemies are prefab variants of one base with patrol, telegraph, attack and
recover states. Rooms are prefabs with a Tilemap and door anchors. Weapons, enemies, rooms and tuning
are ScriptableObjects. Spawned objects are pooled. The WebGL build stays under 50 MB.
**Why:** Kinematic movement gives exact control over feel, and frame-accurate hitboxes come from the
animation itself.

## 2026-11-04: M0 order
**Decision:** Build combat before level generation: M0.1 movement, M0.2 melee and one enemy, M0.3
rooms joined by seed with death and restart.
**Alternatives considered:** Level generation first.
**Why:** How the combat feels is the largest risk to the game, so it is tested first.

## 2026-11-04: Weapons define how combat handles
**Decision:** A weapon is a data asset with a combo of steps, how fast the player moves while
swinging and whether jumping is allowed. Each step plays a part of the shared attack clip at its own
speed. A swing keeps its side, and the next step can turn around.
**Why:** After playing M0.2, stopping to attack felt stiff. Light weapons now let the player walk and
jump while attacking, and heavy ones commit the player. The sword walks at 80% speed.
**Consequences:** The body keeps the attack pose while walking, because the art has no separate legs.

## 2026-11-04: Regular enemies match the player's size
**Decision:** Regular enemies are about as tall as the player, around 2 units. Only bosses are larger.
**Consequences:** The skeleton imports at 24 pixels per unit, so its pixels are slightly smaller than
the player's.

## 2026-11-04: Character colliders have flat bottoms
**Decision:** The player and enemies use box colliders instead of capsules.
**Why:** A capsule's rounded bottom could rest on a platform corner, which counted as ground and held
the player beside the ledge. A flat bottom either stands on the edge or falls past it.

## 2026-11-04: Run structure for M0
**Decision:** A run is a straight chain: a start room, three rooms picked by seed from a pool of four,
and an exit room. Enemy spawns are also picked by seed. Death and the exit both start a new run with a
new nine-digit seed, and the seed is shown on screen.
**Consequences:** Branching paths come after M0. Rebuilding the scene keeps every run clean.

## 2026-11-04: Physics sync after building a run
**Decision:** The run syncs physics transforms after placing rooms, and movement only counts surfaces
that face the direction of travel.
**Why:** Rooms are moved after they are created, so the first physics step could still see their
colliders at the origin. Enemies then fell a few pixels into contact with the floor, and sideways
casts treated the floor as a wall, which left them standing still.

## 2026-11-04: Pixel scale
**Decision:** Sprites import at 16 pixels per unit with point filtering, on a 320x180 reference view.
**Why:** It matches 16-pixel tiles and scales cleanly to 1280x720. To be revisited if the art uses
another resolution.
