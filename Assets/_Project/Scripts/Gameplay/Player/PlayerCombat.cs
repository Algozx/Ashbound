using System.Collections;
using Ashbound.Gameplay.Combat;
using Ashbound.Logic.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ashbound.Gameplay.Player
{
    /// <summary>The player's attack and health.</summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerCombat : MonoBehaviour, IDamageable
    {
        [SerializeField] private AttackDefinition attack;
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
        [SerializeField] private float attackBufferTime = 0.15f;

        private PlayerController controller;
        private AttackTimer attackTimer;
        private Health health;

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
            attackTimer = new AttackTimer(attackBufferTime);
            health = new Health(maxHealth);
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
                attackTimer.Press();
            }

            if (controller.IsDashing && attackTimer.IsActive)
            {
                EndAttack();
            }

            bool canStart = !health.IsDead && !controller.IsDashing && !controller.IsStunned;
            if (attackTimer.Tick(Time.deltaTime, attack.Duration, canStart))
            {
                StartAttack();
            }

            controller.Attacking = attackTimer.IsActive;
            if (!attackTimer.IsActive)
            {
                hitbox.Close();
            }
        }

        private void StartAttack()
        {
            hitbox.Configure(new HitInfo { Damage = attack.Damage, Knockback = attack.Knockback }, controller.Facing);
            controller.RestartAttackAnimation();
            feedback.PlaySwing();
        }

        private void EndAttack()
        {
            attackTimer.Cancel();
            hitbox.Close();
            controller.Attacking = false;
        }

        private void OnHitLanded()
        {
            feedback.PlayHit(attack.HitStop, attack.ShakeForce);
        }

        private IEnumerator RestartAfterDeath()
        {
            yield return new WaitForSeconds(restartDelay);
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
