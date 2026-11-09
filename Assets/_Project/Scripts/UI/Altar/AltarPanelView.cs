using System.Collections.Generic;
using Ashbound.Gameplay.Meta;
using Ashbound.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ashbound.UI.Altar
{
    /// <summary>The list of unlocks shown at an altar. The game waits while it is open.</summary>
    public sealed class AltarPanelView : MonoBehaviour
    {
        [SerializeField] private Progression progression;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private GameObject window;
        [SerializeField] private Transform rowParent;
        [SerializeField] private UnlockRowView rowPrefab;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TMP_Text embersLabel;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference interactAction;

        private readonly List<UnlockRowView> rows = new List<UnlockRowView>();
        private int openedFrame;
        private bool closing;

        private void Awake()
        {
            window.SetActive(false);
            foreach (UnlockDefinition unlock in progression.Unlocks)
            {
                UnlockRowView row = Instantiate(rowPrefab, rowParent);
                row.Bind(unlock, () => Buy(unlock));
                rows.Add(row);
            }

            leaveButton.onClick.AddListener(() => closing = true);
        }

        private void OnEnable()
        {
            progression.AltarOpened += Open;
        }

        private void OnDisable()
        {
            progression.AltarOpened -= Open;
        }

        private void Open()
        {
            openedFrame = Time.frameCount;
            player.SetInputEnabled(false);
            window.SetActive(true);
            cancelAction.action.Enable();
            Refresh();
            SelectFirstAvailable();
        }

        private void Update()
        {
            if (!window.activeSelf)
            {
                return;
            }

            // Hit stop restores the time scale on its own, so the pause is held here.
            Time.timeScale = 0f;

            bool pressed = cancelAction.action.WasPressedThisFrame() || interactAction.action.WasPressedThisFrame();
            if (pressed && Time.frameCount != openedFrame)
            {
                closing = true;
            }
        }

        // Closing after every Update stops the same press from opening the altar again.
        private void LateUpdate()
        {
            if (!closing)
            {
                return;
            }

            closing = false;
            window.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            Time.timeScale = 1f;
            player.SetInputEnabled(true);
        }

        private void Buy(UnlockDefinition unlock)
        {
            if (progression.TryBuy(unlock))
            {
                Refresh();
                SelectFirstAvailable();
            }
        }

        private void Refresh()
        {
            embersLabel.text = "Embers  " + progression.Wallet.Total;
            for (int i = 0; i < rows.Count; i++)
            {
                UnlockDefinition unlock = progression.Unlocks[i];
                rows[i].Show(unlock, progression.Level(unlock), progression.NextCost(unlock), progression.Wallet.Total);
            }
        }

        private void SelectFirstAvailable()
        {
            foreach (UnlockRowView row in rows)
            {
                if (row.Button.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(row.Button.gameObject);
                    return;
                }
            }

            EventSystem.current.SetSelectedGameObject(leaveButton.gameObject);
        }
    }
}
