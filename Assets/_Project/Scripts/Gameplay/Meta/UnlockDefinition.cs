using UnityEngine;

namespace Ashbound.Gameplay.Meta
{
    /// <summary>What a permanent unlock changes.</summary>
    public enum UnlockKind
    {
        MaxHealth,
        Flask,
        Loot,
    }

    /// <summary>A permanent unlock sold at altars. Each cost is one level.</summary>
    [CreateAssetMenu(menuName = "Ashbound/Unlock Definition", fileName = "UnlockDefinition")]
    public sealed class UnlockDefinition : ScriptableObject
    {
        [Tooltip("Key in the saved profile. Changing it loses what players bought.")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private UnlockKind kind;
        [Tooltip("Health per level, or flask charges per level.")]
        [SerializeField] private int amountPerLevel = 1;
        [SerializeField] private int[] costs = { 50 };

        public string Id => id;

        public string DisplayName => displayName;

        public string Description => description;

        public UnlockKind Kind => kind;

        public int AmountPerLevel => amountPerLevel;

        public int[] Costs => costs;
    }
}
