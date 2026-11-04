# Art guide

How to add art so it works in the game without extra setup.

## Where files go
Everything goes under `Assets/_Project/Art/`.

| Folder | Content |
|---|---|
| `Characters/Player` | Player sprite sheets |
| `Enemies/<EnemyName>` | One folder per enemy |
| `Bosses/<BossName>` | One folder per boss |
| `Weapons`, `Items` | Weapon sprites and pickups |
| `Tiles/<BiomeName>` | Tilesets |
| `Backgrounds/<BiomeName>` | Parallax layers |
| `Props`, `VFX`, `UI` | Decoration, effects, interface |

Audio goes in `Assets/_Project/Audio/Music` and `Audio/Sfx`.

## Format
- PNG with transparency, or Aseprite and PSD files, which Unity imports directly.
- Scale: 16 pixels equal one world unit. A 16x16 tile is one unit, and the view is 320x180 pixels.
- Textures in `Art/` get these settings on first import: Sprite type, 16 pixels per unit, point
  filter, no compression. Sprite sheets still need slicing in the Sprite Editor.
- File names: `player_run.png`, `skeleton_attack.png`, `crypt_tileset.png`.

## Animations the code will expect
Player: idle, run, jump, fall, dodge, attack (at least 2 hits in a combo), hurt, death.
Enemy: idle, walk, attack with a readable wind-up, hurt, death.

Melee attacks need their own frames. A character that only shoots does not cover the attack set.

## Sorting layers
`Background`, `Ground`, `Characters`, `Foreground`, `VFX`.

## Licenses
Every asset needs an author, a source link and a license in the README credits table before it is
committed. The repository is public, so Asset Store art cannot be committed without the publisher's
permission. Keep such files out of Git, or use CC0 or your own art.
