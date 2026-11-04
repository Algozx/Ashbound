# Coding rules

- C# for Unity 6000.6. Namespaces follow folders, with no redundant prefixes.
- Game rules go in plain C# classes in `Ashbound.Logic`. MonoBehaviours only bind scene and logic.
- Types get a short English `<summary>` saying what they are for. Members get one line when the name
  is not enough. The reasons behind choices live in `DECISIONS.md`.
- Names use game-development vocabulary and describe what the thing does.
- No balance literals in code. Values are read from data assets.
- No allocations in per-frame paths. Cache components and pool spawned objects.
- All run content uses `SeededRandom`, never `UnityEngine.Random` or `System.Random`.
- UI is a uGUI prefab with a glue script. Shared UI behaviour goes in `UI/Core`.
- Never name a namespace like a BCL type (Math, Random, Tasks).
