using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Makes a glow swell and fade, as the inside of a portal does.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GlowPulse : MonoBehaviour
    {
        [SerializeField] private float speed = 2.5f;
        [SerializeField, Range(0f, 1f)] private float alphaDepth = 0.3f;
        [SerializeField, Range(0f, 1f)] private float sizeDepth = 0.08f;

        private SpriteRenderer glow;
        private Color baseColor;
        private Vector3 baseScale;

        private void Awake()
        {
            glow = GetComponent<SpriteRenderer>();
            baseColor = glow.color;
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * speed);
            Color color = baseColor;
            color.a = baseColor.a * (1f + alphaDepth * wave);
            glow.color = color;
            transform.localScale = baseScale * (1f + sizeDepth * wave);
        }
    }
}
