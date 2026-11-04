# Ashbound Charter

Rules that hold for the whole project. A change needs an entry in `DECISIONS.md`.

## 1. What this is
Ashbound is a 2D side-scrolling action roguelite for the browser. Each run is short and fast, death
restarts the run, and permanent unlocks carry over. It is a portfolio project, published with its
source code on GitHub and playable through GitHub Pages.

## 2. Pillars
1. Combat that feels good within the first minute: fast movement, responsive attacks, a dodge that
   always reads clearly.
2. Every run is different: levels are assembled from hand-made rooms by a seed, and loot changes the
   build.
3. Death moves the player forward: something earned in a failed run stays.

## 3. Non-negotiables
1. Original identity. Genre mechanics are fine. Names, characters, art, music, text and level content
   from other games are not.
2. Rules live in plain C#, presentation in Unity. Game rules are testable without the Editor.
3. Numbers are data. Balance values live in data assets, not in scattered constants.
4. UI is a uGUI prefab with a glue script, unless a decision says otherwise.
5. Design, architecture and scope decisions go to `DECISIONS.md` before code.
6. No feature creep. Ideas outside the current milestone go to `BACKLOG.md`.
7. Every third-party asset has a known author, source and license before it enters the repository.

## 4. Locked stack
| Layer | Choice | Why |
|---|---|---|
| Engine | Unity 6000.6.0f1, URP 2D Renderer | Version installed, 2D lights and tilemaps included |
| Input | Input System | One action map for keyboard, mouse and gamepad |
| Camera | Cinemachine | Follow, confiner and shake without custom code |
| Platform | WebGL on GitHub Pages | A reader can play it from a link |
| Backend | None | Single player, saves stay in the browser |

## 5. Game identity
Setting and tone: to be defined with the first art pass.
Signature mechanic: to be defined in the M0 proposal.

## 6. Definition of done for the first playable
One biome assembled from a seed, one weapon, two enemy types, death and restart, running in a
desktop browser from the GitHub Pages link at 60 FPS.
