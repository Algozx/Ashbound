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

| Asset | Author | License |
|---|---|---|
| No third-party assets yet | | |
