using Ashbound.Gameplay.Levels;
using TMPro;
using UnityEngine;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows how the run ended and how many Embers were kept.</summary>
    public sealed class RunEndView : MonoBehaviour
    {
        [SerializeField] private RunDirector run;
        [SerializeField] private GameObject window;
        [SerializeField] private TMP_Text messageLabel;

        private void Awake()
        {
            window.SetActive(false);
        }

        private void OnEnable()
        {
            run.RunEnded += Show;
        }

        private void OnDisable()
        {
            run.RunEnded -= Show;
        }

        private void Show(string message)
        {
            messageLabel.text = message;
            window.SetActive(true);
        }
    }
}
