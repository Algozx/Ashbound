using System;
using Ashbound.Gameplay.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Altar
{
    /// <summary>One unlock in the altar list.</summary>
    public sealed class UnlockRowView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text costLabel;

        public Button Button => button;

        /// <summary>Sets what this row sells and what a click does.</summary>
        public void Bind(UnlockDefinition unlock, Action onBuy)
        {
            descriptionLabel.text = unlock.Description;
            button.onClick.AddListener(() => onBuy());
        }

        /// <summary>Shows the current level and whether the player can pay for the next one.</summary>
        public void Show(UnlockDefinition unlock, int level, int cost, int embers)
        {
            bool hasLevels = unlock.Costs.Length > 1;
            nameLabel.text = hasLevels ? unlock.DisplayName + "  " + level + "/" + unlock.Costs.Length : unlock.DisplayName;
            costLabel.text = cost < 0 ? "Owned" : cost.ToString();
            button.interactable = cost >= 0 && cost <= embers;
        }
    }
}
