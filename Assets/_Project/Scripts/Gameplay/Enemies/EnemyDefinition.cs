using Ashbound.Logic.Enemies;
using UnityEngine;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>Values of one enemy type.</summary>
    [CreateAssetMenu(menuName = "Ashbound/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 40;
        [SerializeField] private int damage = 10;
        [SerializeField] private float knockback = 7f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField, Range(0f, 1f)] private float patrolSpeedScale = 0.5f;
        [SerializeField] private float gravity = 40f;
        [SerializeField] private float knockbackDecay = 30f;

        [Header("Senses")]
        [SerializeField] private float aggroRange = 9f;
        [SerializeField] private float aggroHeight = 3f;
        [SerializeField] private float attackRange = 2f;

        [Header("Attack rhythm")]
        [SerializeField] private float telegraphTime = 0.5f;
        [SerializeField] private float strikeTime = 0.17f;
        [SerializeField] private float recoverTime = 0.7f;
        [SerializeField] private float hurtTime = 0.3f;

        public int MaxHealth => maxHealth;

        public int Damage => damage;

        public float Knockback => knockback;

        public float MoveSpeed => moveSpeed;

        public float Gravity => gravity;

        public float KnockbackDecay => knockbackDecay;

        public EnemyBrainSettings ToBrainSettings()
        {
            return new EnemyBrainSettings
            {
                PatrolSpeedScale = patrolSpeedScale,
                AggroRange = aggroRange,
                AggroHeight = aggroHeight,
                AttackRange = attackRange,
                TelegraphTime = telegraphTime,
                StrikeTime = strikeTime,
                RecoverTime = recoverTime,
                HurtTime = hurtTime,
            };
        }
    }
}
