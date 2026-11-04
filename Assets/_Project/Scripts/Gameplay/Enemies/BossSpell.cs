using System;
using Ashbound.Gameplay.Combat;
using UnityEngine;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>
    /// A portal that opens over a spot and strikes it a moment later. The damage window comes from
    /// animation events, so the portal itself is the warning.
    /// </summary>
    public sealed class BossSpell : MonoBehaviour
    {
        private static readonly int SpellState = Animator.StringToHash("Spell");

        [SerializeField] private Animator animator;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private float lifetime = 1.6f;

        private Action<BossSpell> release;
        private float age;

        /// <summary>Starts a pooled spell at a spot on the ground.</summary>
        public void Launch(Vector2 position, in HitInfo hit, Action<BossSpell> onFinished)
        {
            transform.position = position;
            hitbox.Configure(hit, 1);
            release = onFinished;
            age = 0f;
            animator.Play(SpellState, 0, 0f);
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                hitbox.Close();
                release(this);
            }
        }
    }
}
