# Ashbound Game Design Document

Living document, kept short. Only approved changes go here.

## 1. Pitch
A 2D side-scrolling action roguelite for the browser. Short, fast runs through levels assembled from
hand-made rooms, where death restarts the run and permanent unlocks carry over.

## 2. Pillars
1. Combat that feels good within the first minute.
2. Every run is different.
3. Death moves the player forward.

## 3. Loops
- Core (seconds): move, dodge, attack, get feedback, collect.
- Run (minutes): enter a biome, fight through rooms, pick up gear, face the boss or die.
- Meta (across runs): spend what survived the run on permanent unlocks.

## 4. Target
Desktop browsers, keyboard and mouse or gamepad. Runs of 10 to 20 minutes.

## 5. Systems
| System | One line | Status |
|---|---|---|
| Player movement | Run, jump, dodge | Idea |
| Melee combat | Attacks, hit reactions, hit stop | Idea |
| Enemies | Telegraphed attacks, simple states | Idea |
| Level assembly | Rooms joined by a seeded graph | Idea |
| Loot | Weapons and skills found during a run | Idea |
| Meta progression | Embers spent at altars on permanent unlocks | Built |

## 6. Content plan
| | First playable | Release |
|---|---|---|
| Biomes | 1 | To be decided |
| Weapons | 1 | To be decided |
| Enemy types | 2 | To be decided |
| Bosses | 0 | To be decided |

## 7. Signature
Heat. Landing hits without being hit builds three levels, which raise damage and the Embers enemies
drop. Taking damage puts it out.

## 8. Out of scope
See `BACKLOG.md`.
