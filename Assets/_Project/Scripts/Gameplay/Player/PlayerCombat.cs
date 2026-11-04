using System.Collections;
using Ashbound.Gameplay.Combat;
using Ashbound.Logic.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ashbound.Gameplay.Player
{
    /// <summary>The player's weapon and health.</summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombat : MonoBehaviour, IDamageable
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private CombatFeedback feedback;

        [Header("Health")]
        [SerializeField] private int maxHealth = 50;
        [SerializeField] private float hurtStun = 0.3f;
        [SerializeField] private float hurtInvulnerability = 1f;
        [SerializeField] private float hurtLift = 5f;
        [SerializeField] private float hurtStop = 0.1f;
        [SerializeField] private float hurtShake = 0.5f;
        [SerializeField] private float restartDelay = 1.6f;

        [Header("Input")]
        [SerializeField] private float attackBufferTime = 0.2f;

        private PlayerController controller;
        private AttackCombo combo;
        private Health health;
        private float[] stepDurations;
        private AttackStep currentStep;

        public int CurrentHealth => health.Current;

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

            if (killed)
            {
                controller.Die();
                StartCoroutine(RestartAfterDeath());
            }

            return true;
        }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            combo = new AttackCombo(attackBufferTime, weapon.ComboWindow);
            health = new Health(maxHealth);
            stepDurations = weapon.StepDurations();
        }

        private void OnEnable()
        {
            attackAction.action.Enable();
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

        private IEnumerator RestartAfterDeath()
        {
            yield return new WaitForSeconds(restartDelay);
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
