using System;
using Ashbound.Gameplay.Meta;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>One thing a room can offer, and how often.</summary>
    [Serializable]
    public sealed class LootEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int weight = 1;
        [Tooltip("Leave empty for loot that is always available.")]
        [SerializeField] private UnlockDefinition requiredUnlock;

        public GameObject Prefab => prefab;

        public int Weight => weight;

        public UnlockDefinition RequiredUnlock => requiredUnlock;
    }
}
