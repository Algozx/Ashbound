using System;
using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>
    /// How a weapon handles: its combo, how much the player can move while swinging and whether
    /// jumping is allowed. Weapons reuse one attack clip, so each step plays a part of it.
    /// </summary>
    [CreateAssetMenu(menuName = "Ashbound/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private Sprite icon;
        [SerializeField] private AnimationClip attackClip;
        [SerializeField, Range(0f, 1f)] private float moveScale = 0.8f;
        [SerializeField] private bool blocksJump;
        [SerializeField] private float comboWindow = 0.35f;
        [SerializeField] private AttackStep[] steps;

        public string DisplayName => displayName;

        public Sprite Icon => icon;

        public float MoveScale => moveScale;

        public bool BlocksJump => blocksJump;

        public float ComboWindow => comboWindow;

        public AttackStep[] Steps => steps;

        public float ClipLength => attackClip.length;

        /// <summary>Durations of the steps, in combo order.</summary>
        public float[] StepDurations()
        {
            var durations = new float[steps.Length];
            for (int i = 0; i < steps.Length; i++)
            {
                durations[i] = steps[i].Duration;
            }

            return durations;
        }
    }

    /// <summary>One attack of a combo.</summary>
    [Serializable]
    public sealed class AttackStep
    {
        [Tooltip("Part of the attack clip this step plays, in seconds.")]
        [SerializeField] private float clipStart;
        [SerializeField] private float clipEnd = 0.6f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float knockback = 6f;
        [Tooltip("Forward push when the step starts, in units per second.")]
        [SerializeField] private float lunge = 3f;
        [SerializeField] private float hitStop = 0.04f;
        [SerializeField] private float shakeForce = 0.2f;

        public float ClipStart => clipStart;

        public float ClipEnd => clipEnd;

        public float Duration => duration;

        public int Damage => damage;

        public float Knockback => knockback;

        public float Lunge => lunge;

        public float HitStop => hitStop;

        public float ShakeForce => shakeForce;
    }
}
