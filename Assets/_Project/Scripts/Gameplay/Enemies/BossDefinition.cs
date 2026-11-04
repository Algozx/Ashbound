using Ashbound.Logic.Enemies;
using UnityEngine;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>Values of the boss.</summary>
    [CreateAssetMenu(menuName = "Ashbound/Boss Definition", fileName = "BossDefinition")]
    public sealed class BossDefinition : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private string displayName = "Boss";
        [SerializeField] private int maxHealth = 400;
        [SerializeField] private int meleeDamage = 22;
        [SerializeField] private int spellDamage = 18;
        [SerializeField] private float knockback = 9f;
        [SerializeField] private float moveSpeed = 2.5f;

        [Header("Rhythm")]
        [SerializeField] private float meleeRange = 3.5f;
        [SerializeField] private float meleeWindup = 0.4f;
        [SerializeField] private float meleeStrike = 0.4f;
        [SerializeField] private float castWindup = 0.5f;
        [SerializeField] private float castRelease = 0.4f;
        [SerializeField] private float recover = 1f;
        [SerializeField] private float castInterval = 6f;
        [SerializeField] private int meleesBeforeCast = 2;

        [Header("Second phase")]
        [SerializeField, Range(0f, 1f)] private float enrageHealthShare = 0.5f;
        [SerializeField, Range(0f, 1f)] private float enragedRecoverScale = 0.6f;

        [Header("Spells")]
        [SerializeField] private int spellsPerCast = 1;
        [SerializeField] private int enragedSpellsPerCast = 3;
        [SerializeField] private float spellSpacing = 3.5f;

        public string DisplayName => displayName;

        public int MaxHealth => maxHealth;

        public int MeleeDamage => meleeDamage;

        public int SpellDamage => spellDamage;

        public float Knockback => knockback;

        public float MoveSpeed => moveSpeed;

        public float SpellSpacing => spellSpacing;

        public BossBrainSettings ToBrainSettings()
        {
            return new BossBrainSettings
            {
                MeleeRange = meleeRange,
                MeleeWindup = meleeWindup,
                MeleeStrike = meleeStrike,
                CastWindup = castWindup,
                CastRelease = castRelease,
                Recover = recover,
                CastInterval = castInterval,
                MeleesBeforeCast = meleesBeforeCast,
                EnrageHealthShare = enrageHealthShare,
                EnragedRecoverScale = enragedRecoverScale,
                SpellsPerCast = spellsPerCast,
                EnragedSpellsPerCast = enragedSpellsPerCast,
            };
        }
    }
}
