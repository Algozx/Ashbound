using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Makes a light breathe like a flame.</summary>
    [RequireComponent(typeof(Light2D))]
    public sealed class LightFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float depth = 0.25f;
        [SerializeField] private float speed = 5f;

        private Light2D flame;
        private float baseIntensity;
        private float phase;

        private void Awake()
        {
            flame = GetComponent<Light2D>();
            baseIntensity = flame.intensity;
            phase = Random.value * 100f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(phase + Time.time * speed, 0f);
            flame.intensity = baseIntensity * (1f - depth + depth * 2f * noise);
        }
    }
}
