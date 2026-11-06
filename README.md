# Ashbound

A 2D side-scrolling action roguelite for the browser, built in Unity. You fight through a castle in
ashes, keep part of what you earn when you die, and come back stronger.

## Controls

| Action | Keyboard and mouse | Gamepad |
|---|---|---|
| Move | WASD or arrow keys | Left stick or D-pad |
| Jump | Space | South button |
| Drop through a platform | Down and Jump | Down and South button |
| Dash | Left Shift | East button or left shoulder |
| Attack | J or left mouse button | West button |
| Interact | F | Right shoulder |
| Healing flask, once unlocked | Q | Left trigger |
| Map | M | Select |

The gamepad is mapped but was not tested on real hardware. Touch is not supported.

## About

I built Ashbound as a portfolio project, to work through the systems of an action roguelite in Unity.

A run starts in a hub and goes through two large hand-made maps, with a rest room after each one and a
boss at the end. A seed picks the maps, the enemies and the treasure. Each map has a normal exit and a
danger exit, which makes what comes next harder and better paid.

Enemies drop Embers. Dying keeps half of what you carry and beating the boss keeps all of it, so
spending at an altar before a risky fight protects them. Embers buy permanent unlocks, saved in the
browser. Landing hits without being hit builds Heat, which raises damage and Embers until you get hit.

Project documents are in `design/`, and the reasons behind each choice are in `DECISIONS.md`.

## Running the project

Open it with Unity 6000.6.0f1 or newer, open `Assets/_Project/Scenes/Bootstrap.unity` and press Play.
Tests run in the Test Runner, under EditMode.

To produce the WebGL build, use `Ashbound > Build WebGL`. It writes to `docs/`, the folder served by
GitHub Pages.

## Credits

Asset Store art is licensed for use in games but not for redistribution, so it lives in a private
submodule at `Assets/_Project/Art`. A clone of this repository compiles but shows no art.

| Asset | Author | License |
|---|---|---|
| [Rogue Fantasy Castle](https://assetstore.unity.com/packages/2d/environments/rogue-fantasy-castle-164725) | Szadi art | Unity Asset Store |
| Warrior, Bringer of Death | Clembod | Free for commercial use, no redistribution |
| [Pixel Art Icon Pack - RPG](https://assetstore.unity.com/packages/slug/158343) | Cainos | Unity Asset Store |
| [Monsters Creatures Fantasy](https://luizmelo.itch.io/monsters-creatures-fantasy) | Luiz Melo | CC0 |
| [Tiny RPG Font Kit I](https://opengameart.org/content/tiny-rpg-font-kit-i) | tiopalada | CC0 |
| [RPG Sound Pack](https://opengameart.org/content/rpg-sound-pack) | artisticdude | CC0 |
| [50 RPG sound effects](https://opengameart.org/content/50-rpg-sound-effects) | Kenney | CC0 |
| [Interface Sounds](https://opengameart.org/content/interface-sounds) | Kenney | CC0 |
| [Dark Shrine Loop](https://opengameart.org/content/dark-shrine-loop) | yd, remixed by qubodup | CC0 |
| [Dark Cavern Ambient](https://opengameart.org/content/dark-cavern-ambient) | Paul Wortmann | CC0 |
