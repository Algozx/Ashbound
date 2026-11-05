using System;
using Ashbound.Gameplay.Player;
using Ashbound.Logic.Meta;
using UnityEngine;
using UnityEngine.Pool;

namespace Ashbound.Gameplay.Meta
{
    /// <summary>Connects the saved profile to a run: Embers, unlock effects and purchases.</summary>
    public sealed class Progression : MonoBehaviour
    {
        private const int EmbersPerOrb = 3;
        private const int MaxOrbs = 8;
        private const float DropHeight = 1f;

        [SerializeField] private UnlockDefinition[] unlocks;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private PlayerFlask flask;
        [SerializeField] private EmberPickup emberPrefab;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] emberSounds;
        [SerializeField, Range(0f, 1f)] private float emberVolume = 0.3f;
        [Tooltip("Shortest gap between Ember sounds, so a burst does not stack them.")]
        [SerializeField] private float emberSoundGap = 0.09f;
        [SerializeField] private AudioClip buySound;

        private Profile profile;
        private EmberWallet wallet;
        private ObjectPool<EmberPickup> embers;
        private float nextEmberSound;

        public UnlockDefinition[] Unlocks => unlocks;

        public EmberWallet Wallet
        {
            get
            {
                LoadProfile();
                return wallet;
            }
        }

        /// <summary>Raised when carried or banked Embers change.</summary>
        public event Action EmbersChanged;

        /// <summary>Raised when the player uses an altar.</summary>
        public event Action AltarOpened;

        public void OpenAltar()
        {
            AltarOpened?.Invoke();
        }

        public int Level(UnlockDefinition unlock)
        {
            LoadProfile();
            return profile.Level(unlock.Id);
        }

        /// <summary>True when nothing is required, or when the unlock was bought.</summary>
        public bool IsUnlocked(UnlockDefinition unlock)
        {
            return unlock == null || Level(unlock) > 0;
        }

        /// <summary>Price of the next level, or -1 when the unlock is complete.</summary>
        public int NextCost(UnlockDefinition unlock)
        {
            return Wallet.NextCost(unlock.Id, unlock.Costs);
        }

        /// <summary>Buys one level, saves it and applies it at once.</summary>
        public bool TryBuy(UnlockDefinition unlock)
        {
            if (!Wallet.TryBuy(unlock.Id, unlock.Costs))
            {
                return false;
            }

            ProfileStore.Save(profile);
            audioSource.PlayOneShot(buySound);
            Apply(unlock, 1);
            EmbersChanged?.Invoke();
            return true;
        }

        /// <summary>Bursts Embers out of a spot, split into a few orbs.</summary>
        public void DropEmbers(Vector2 position, int amount)
        {
            int orbs = Mathf.Clamp(amount / EmbersPerOrb, 1, MaxOrbs);
            for (int i = 0; i < orbs; i++)
            {
                int value = amount / orbs + (i < amount % orbs ? 1 : 0);
                embers.Get().Launch(position + Vector2.up * DropHeight, value, player.transform, OnEmberCollected);
            }
        }

        /// <summary>Ends the run in a loss and returns the Embers kept.</summary>
        public int SettleDeath()
        {
            return Settled(Wallet.SettleDeath());
        }

        /// <summary>Ends the run in a win and returns the Embers kept.</summary>
        public int SettleVictory()
        {
            return Settled(Wallet.SettleVictory());
        }

        private void Awake()
        {
            embers = new ObjectPool<EmberPickup>(
                () => Instantiate(emberPrefab, transform),
                ember => ember.gameObject.SetActive(true),
                ember => ember.gameObject.SetActive(false));
        }

        private void Start()
        {
            foreach (UnlockDefinition unlock in unlocks)
            {
                Apply(unlock, Level(unlock));
            }
        }

        // The run director asks for unlocks in its own Awake, which can come before this one.
        private void LoadProfile()
        {
            if (profile == null)
            {
                profile = ProfileStore.Load();
                wallet = new EmberWallet(profile);
            }
        }

        private void Apply(UnlockDefinition unlock, int levels)
        {
            int amount = unlock.AmountPerLevel * levels;
            if (amount <= 0)
            {
                return;
            }

            if (unlock.Kind == UnlockKind.MaxHealth)
            {
                player.RaiseMaxHealth(amount);
            }
            else if (unlock.Kind == UnlockKind.Flask)
            {
                flask.AddCapacity(amount);
            }
        }

        private void OnEmberCollected(EmberPickup ember)
        {
            Wallet.Collect(ember.Value);
            if (Time.time >= nextEmberSound)
            {
                nextEmberSound = Time.time + emberSoundGap;
                audioSource.PlayOneShot(emberSounds[UnityEngine.Random.Range(0, emberSounds.Length)], emberVolume);
            }

            embers.Release(ember);
            EmbersChanged?.Invoke();
        }

        private int Settled(int kept)
        {
            ProfileStore.Save(profile);
            EmbersChanged?.Invoke();
            return kept;
        }
    }
}
