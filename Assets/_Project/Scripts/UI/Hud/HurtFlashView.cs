using Ashbound.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Flashes the screen red when the player takes a hit.</summary>
    public sealed class HurtFlashView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat player;
        [SerializeField] private Image flash;
        [SerializeField, Range(0f, 1f)] private float strength = 0.35f;
        [SerializeField] private float fadeTime = 0.3f;

        private float alpha;

        private void OnEnable()
        {
            player.Hurt += OnHurt;
        }

        private void OnDisable()
        {
            player.Hurt -= OnHurt;
        }

        private void OnHurt()
        {
            alpha = strength;
        }

        // Unscaled, because hit stop freezes the game at the moment of the hit.
        private void Update()
        {
            alpha = Mathf.MoveTowards(alpha, 0f, strength / fadeTime * Time.unscaledDeltaTime);
            Color color = flash.color;
            color.a = alpha;
            flash.color = color;
        }
    }
}
