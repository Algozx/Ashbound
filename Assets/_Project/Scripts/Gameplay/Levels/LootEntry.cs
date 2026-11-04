using System;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>One thing a room can offer, and how often.</summary>
    [Serializable]
    public sealed class LootEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int weight = 1;

        public GameObject Prefab => prefab;

        public int Weight => weight;
    }
}
