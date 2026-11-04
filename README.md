# Ashbound

A 2D side-scrolling action roguelite for the browser, built in Unity.

The project is in its first stage. The foundation is in place and the core loop comes next, so there
is no playable build yet.

## Controls

Planned input, already mapped in the Input System:

| Action | Keyboard and mouse | Gamepad |
|---|---|---|
| Move | WASD or arrow keys | Left stick or D-pad |
| Jump | Space | South button |
| Dodge | Left Shift | East button or left shoulder |
| Attack | J or left mouse button | West button |
| Secondary attack | K or right mouse button | North button |
| Skills | Q and E | Left and right triggers |
| Interact | F | Right shoulder |
| Pause | Escape or P | Start |

## About

I am building Ashbound as a portfolio project. The goal is short runs through levels assembled from
hand-made rooms, with a seed that reproduces any run. Game rules live in a C# assembly with no engine
references, so they run in EditMode tests. Design notes and decisions are in `design/` and
`DECISIONS.md`.

## Running the project

Open it with Unity 6000.6.0f1 or newer, open `Assets/_Project/Scenes/Bootstrap.unity` and press Play.

To produce the WebGL build, use `Ashbound > Build WebGL`. It writes to `docs/`, the folder served by
GitHub Pages.

## Credits

Asset Store art is licensed for use in games but not for redistribution, so it lives in a private
submodule at `Assets/_Project/Art`. A clone of this repository compiles but shows no art.

| Asset | Author | License |
|---|---|---|
| [Monsters Creatures Fantasy](https://luizmelo.itch.io/monsters-creatures-fantasy) | Luiz Melo | CC0 |
| [Pixel art castle tileset](https://opengameart.org/content/pixel-art-castle-tileset) | rubberduck | CC0 |
| [Tiny RPG Font Kit I](https://opengameart.org/content/tiny-rpg-font-kit-i) | tiopalada | CC0 |
| [RPG Sound Pack](https://opengameart.org/content/rpg-sound-pack) | artisticdude | CC0 |
| [50 RPG sound effects](https://opengameart.org/content/50-rpg-sound-effects) | Kenney | CC0 |
| [Interface Sounds](https://opengameart.org/content/interface-sounds) | Kenney | CC0 |
| [Dark Shrine Loop](https://opengameart.org/content/dark-shrine-loop) | yd, remixed by qubodup | CC0 |
| [Dark Cavern Ambient](https://opengameart.org/content/dark-cavern-ambient) | Paul Wortmann | CC0 |
| Warrior, Bringer of Death | Clembod | Unity Asset Store |
| Pixel Art Icon Pack - RPG | Cainos | Unity Asset Store |
| GUI Fantasy Kit, RF Castle, 500 Free Skill Icons | To be confirmed | Unity Asset Store |
