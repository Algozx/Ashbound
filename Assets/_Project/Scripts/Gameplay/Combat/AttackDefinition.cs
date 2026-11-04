using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Values of one attack. The active frames come from the animation events.</summary>
    [CreateAssetMenu(menuName = "Ashbound/Attack Definition", fileName = "AttackDefinition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private float knockback = 6f;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private float hitStop = 0.06f;
        [SerializeField] private float shakeForce = 0.25f;

        public int Damage => damage;

        public float Knockback => knockback;

        public float Duration => duration;

        public float HitStop => hitStop;

        public float ShakeForce => shakeForce;
    }
}
