using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>What enemies and bosses share on screen: facing, hit flash, effects and animation.</summary>
    public sealed class CharacterView : MonoBehaviour
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Death = Animator.StringToHash("Death");

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem deathEffect;
        [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private float flashTime = 0.1f;
        [Tooltip("On for sprite sheets drawn facing left.")]
        [SerializeField] private bool spritesFaceLeft;

        private float flashTimer;

        /// <summary>Transparency of the character, for a corpse that fades away.</summary>
        public float Alpha { get; set; } = 1f;

        public void PlayAnimation(int state)
        {
            animator.Play(state, 0, 0f);
        }

        public void ShowHit()
        {
            flashTimer = flashTime;
            hitEffect.Play();
        }

        public void ShowDeath()
        {
            deathEffect.Play();
        }

        /// <summary>Updates the sprite. Call it once per frame, after the character moved.</summary>
        public void Draw(int facing, Color tint)
        {
            spriteRenderer.flipX = spritesFaceLeft ? facing > 0 : facing < 0;
            flashTimer = Mathf.Max(0f, flashTimer - Time.unscaledDeltaTime);

            Color color = flashTimer > 0f ? hitColor : tint;
            color.a = Alpha;
            spriteRenderer.color = color;
        }
    }
}
