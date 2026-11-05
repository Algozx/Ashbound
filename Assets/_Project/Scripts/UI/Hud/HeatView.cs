using Ashbound.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows the player's Heat as a row of markers.</summary>
    public sealed class HeatView : MonoBehaviour
    {
        [SerializeField] private PlayerHeat heat;
        [SerializeField] private Image[] markers;
        [SerializeField] private Color litColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private Color coldColor = new Color(0.25f, 0.2f, 0.25f, 0.9f);

        private void OnEnable()
        {
            heat.LevelChanged += Show;
        }

        private void OnDisable()
        {
            heat.LevelChanged -= Show;
        }

        private void Start()
        {
            Show(heat.Level);
        }

        private void Show(int level)
        {
            for (int i = 0; i < markers.Length; i++)
            {
                markers[i].color = i < level ? litColor : coldColor;
            }
        }
    }
}
