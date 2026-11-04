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

## 2026-11-04: M1 scope
**Decision:** M1 builds challenge first: new enemies and difficulty, loot, a boss, meta progression,
then the real visuals. The goblin is fast, the mushroom is slow and shoots, the flying eye attacks from
the air, and the Bringer of Death is the boss. Enemies get stronger room by room and stay in their
room. The signature mechanic is now chosen in the middle of M1, with loot and enemy variety in play.
**Alternatives considered:** Visuals first.
**Why:** The M0 playtest was fun but lacked challenge, and decorated rooms could need rework when
combat changes.

## 2026-11-04: One-way platforms and climbing enemies
**Decision:** Thin platforms are one-way, on their own layer. Bodies pass through from below and land
from above, and the player drops through with Down and Jump. Skeletons and goblins jump after the
player, over walls and gaps, and walk off ledges to reach a lower target. Mushrooms stay on the ground.
**Why:** After the M1.1 playtest, standing on a platform was too safe. One-way platforms let enemies
reach the player from directly below without pathfinding.

## 2026-11-04: Loot in a run
**Decision:** Each combat room offers one seeded item: a health potion (weight 3) or a weapon (weight 1
each). Potions work on touch and wait if health is full. Weapons are taken with Interact, and the old
weapon stays on the ground, so a run is a choice between handling styles.
**Consequences:** Loot sits on platforms and steps, so reaching it means leaving the fight's safe line.

## 2026-11-04: Boss fight
**Decision:** Every run ends in an arena with the Bringer of Death. Entering closes the door, and the
exit appears after the boss dies. It walks to the player and swings, and it casts on a timer or after
two swings in a row, so distance is not safe. Spells open a portal over the player and strike a moment
later. At half health it recovers faster and casts three spells. Hits never stagger it.
**Why:** A run needs a clear goal, and two attack types force both dodging and repositioning.

## 2026-11-04: Platforms are entered at their ends
**Decision:** The middle of a platform is on its own layer, `PlatformCore`. Bodies land on it and drop
through it, but they cannot rise through it. The end tiles stay one-way, and they are drawn lighter so
the way up is readable. A body whose head is already past the underside keeps rising, so a corner never
catches a jump. Skeletons and goblins under a platform walk to the nearest free column before jumping,
and they ignore targets higher than their jump. Flying enemies ignore the middle.
**Why:** After the M1.3 playtest, passing through a platform anywhere felt confusing. This supersedes
the part of the earlier one-way entry that let bodies pass from below at any point.

## 2026-11-04: Enemies patrol, give up and go home
**Decision:** Enemies walk around their spawn point and rest between walks, varied by the run's seed.
Jumpers climb steps and sometimes drop off ledges on patrol, and flying enemies cruise above their
spawn point. When the player leaves the enemy's area, or there is no way forward for a second, the
enemy walks home and ignores the player for two seconds, unless it is hit. If home cannot be reached,
the spot it stopped at becomes its home.
**Why:** Enemies stood frozen at the limit of their room, which looked like a bug. Patrols also make
rooms feel alive before a fight starts.

## 2026-11-04: Boss swing follows the art
**Decision:** The swing uses two boxes measured from the sprite frames: a low one 6 units long and an
upper one that follows the arc. The damage window covers every frame that shows the scythe, and the
full animation now plays. Hitboxes support an optional second box for this.
**Why:** The old box ended 1.5 units short of the scythe and closed before the animation did, so a
visible hit could miss. The boss now reaches further, which makes the fight harder.

## 2026-11-04: Embers and permanent unlocks
**Decision:** Enemies drop Embers, 3 to 8 each, and the boss drops 50. Dying keeps half of what the
player carries and beating the boss keeps all of it. Embers are spent at an altar in the start room and
at one near the end of every room, so spending before a risky fight protects them. Unlocks are three
levels of maximum health, the dagger and heavy blade joining the loot, and a healing flask carried into
every run. Progress is a versioned JSON profile kept in the browser.
**Alternatives considered:** Keeping everything on death, depositing at altars, and a shop screen after
each death.
**Why:** The user chose losing half with altars at every room change, which makes each altar a decision.
**Consequences:** A seed repeats a run only with the same weapons unlocked. Closing the browser during
a run loses the Embers carried.

## 2026-11-04: Signature mechanic
**Decision:** Heat. Landing hits without being hit builds three levels of Heat, which raise damage and
the Embers each enemy drops. Taking damage puts it out. It is built as its own slice after M1.4.
**Alternatives considered:** A burning dash, which needs new effects and more balancing.
**Why:** It rewards skill, links combat to the currency, and needs no new art.

## 2026-11-04: Hub, maps and rest rooms
**Decision:** The game starts in a hub with an altar and a portal, where NPCs can go later. A run is two
maps of two rooms each, a rest room after each map, then the boss. Portals, used with Interact, join
these parts. A rest room has a fire that restores health and the flask once, an altar and the next
portal. Altars no longer stand at the end of rooms. Death and the exit return to the hub. The hub and
rest rooms are room prefabs in the Game scene, built with the run.
**Alternatives considered:** A separate hub scene, which would duplicate the player and HUD setup before
there is anything to justify it. A rest room after every single room, which would remove most of the
pressure of a run.
**Why:** The user asked for a home base and for a real pause between maps instead of a totem at the end
of every room. This supersedes the altar placement in the Embers entry.
**Consequences:** Runs now have four rooms instead of three, so the last room is harder than before and
Embers per run go up. `mapsPerRun` and `roomsPerMap` are on the run director.

## 2026-11-04: Pixel scale
**Decision:** Sprites import at 16 pixels per unit with point filtering, on a 320x180 reference view.
**Why:** It matches 16-pixel tiles and scales cleanly to 1280x720. To be revisited if the art uses
another resolution.
