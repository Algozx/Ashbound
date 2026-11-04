using System;
using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Pickups;
using Ashbound.Logic.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashbound.Gameplay.Player
{
    /// <summary>The player's weapon and health.</summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombat : MonoBehaviour, IDamageable
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private CombatFeedback feedback;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float hurtStun = 0.3f;
        [SerializeField] private float hurtInvulnerability = 1f;
        [SerializeField] private float hurtLift = 5f;
        [SerializeField] private float hurtStop = 0.1f;
        [SerializeField] private float hurtShake = 0.5f;

        [Header("Input")]
        [SerializeField] private float attackBufferTime = 0.2f;

        private PlayerController controller;
        private PlayerFlask flask;
        private AttackCombo combo;
        private Health health;
        private float[] stepDurations;
        private AttackStep currentStep;
        private Interactable nearby;

        public int CurrentHealth => health.Current;

        public int MaxHealth => health.Max;

        public WeaponDefinition Weapon => weapon;

        /// <summary>Raised when the equipped weapon changes.</summary>
        public event Action<WeaponDefinition> WeaponChanged;

        /// <summary>Raised with what the interact button would use, or null when there is nothing.</summary>
        public event Action<Interactable> NearbyInteractableChanged;

        /// <summary>Raised with current and maximum health after a hit.</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once, on the killing hit.</summary>
        public event Action Died;

        public void Equip(WeaponDefinition newWeapon)
        {
            EndAttack();
            UseWeapon(newWeapon);
            WeaponChanged?.Invoke(weapon);
        }

        public void Heal(int amount)
        {
            health.Heal(amount);
            HealthChanged?.Invoke(health.Current, health.Max);
        }

        /// <summary>Adds to the maximum health, and heals by the same amount.</summary>
        public void RaiseMaxHealth(int amount)
        {
            health.RaiseMax(amount);
            HealthChanged?.Invoke(health.Current, health.Max);
        }

        /// <summary>Tells the player what the interact button would use.</summary>
        public void SetNearbyInteractable(Interactable interactable)
        {
            nearby = interactable;
            NearbyInteractableChanged?.Invoke(interactable);
        }

        /// <summary>Stops or restarts reading the player's input, for menus.</summary>
        public void SetInputEnabled(bool inputEnabled)
        {
            enabled = inputEnabled;
            controller.enabled = inputEnabled;
            flask.enabled = inputEnabled;
        }

        public bool TakeHit(in HitInfo hit)
        {
            if (health.IsDead || controller.IsInvulnerable)
            {
                return false;
            }

            EndAttack();
            bool killed = health.Damage(hit.Damage);
            float direction = transform.position.x >= hit.SourceX ? 1f : -1f;
            controller.Hurt(direction * hit.Knockback, hurtLift, hurtStun, hurtInvulnerability);
            feedback.PlayHurt(hurtStop, hurtShake);
            HealthChanged?.Invoke(health.Current, health.Max);

            if (killed)
            {
                controller.Die();
                Died?.Invoke();
            }

            return true;
        }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            flask = GetComponent<PlayerFlask>();
            health = new Health(maxHealth);
            UseWeapon(weapon);
        }

        private void UseWeapon(WeaponDefinition newWeapon)
        {
            weapon = newWeapon;
            combo = new AttackCombo(attackBufferTime, weapon.ComboWindow);
            stepDurations = weapon.StepDurations();
        }

        private void OnEnable()
        {
            attackAction.action.Enable();
            interactAction.action.Enable();
            hitbox.HitLanded += OnHitLanded;
        }

        private void OnDisable()
        {
            hitbox.HitLanded -= OnHitLanded;
        }

        private void Update()
        {
            if (attackAction.action.WasPressedThisFrame())
            {
                combo.Press();
            }

            if (nearby != null && !health.IsDead && interactAction.action.WasPressedThisFrame())
            {
                nearby.Interact(this);
            }

            if (controller.IsDashing && combo.IsActive)
            {
                EndAttack();
            }

            bool canStart = !health.IsDead && !controller.IsDashing && !controller.IsStunned;
            int step = combo.Tick(Time.deltaTime, stepDurations, canStart);
            if (step != AttackCombo.NoStep)
            {
                StartStep(weapon.Steps[step]);
            }

            controller.SetAttacking(combo.IsActive, weapon.MoveScale, weapon.BlocksJump);
            if (!combo.IsActive)
            {
                hitbox.Close();
            }
        }

        private void StartStep(AttackStep step)
        {
            currentStep = step;
            hitbox.Close();
            controller.FaceInputDirection();
            hitbox.Configure(new HitInfo { Damage = step.Damage, Knockback = step.Knockback }, controller.Facing);
            controller.PlayAttack(step.ClipStart / weapon.ClipLength, (step.ClipEnd - step.ClipStart) / step.Duration);
            controller.Lunge(step.Lunge);
            feedback.PlaySwing();
        }

        private void EndAttack()
        {
            combo.Cancel();
            hitbox.Close();
            controller.SetAttacking(false, 1f, false);
        }

        private void OnHitLanded()
        {
            feedback.PlayHit(currentStep.HitStop, currentStep.ShakeForce);
        }
    }
}
