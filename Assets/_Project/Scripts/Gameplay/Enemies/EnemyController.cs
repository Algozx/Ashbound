using System.Collections;
using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Physics;
using Ashbound.Logic.Combat;
using Ashbound.Logic.Enemies;
using UnityEngine;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>Binds <see cref="EnemyBrain"/> to a body, an animator and a hitbox.</summary>
    [RequireComponent(typeof(KinematicMover))]
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        private const float HitFlashTime = 0.1f;
        private const float LookAhead = 0.1f;
        private const float LedgeProbeDepth = 0.6f;

        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int AttackState = Animator.StringToHash("Attack");
        private static readonly int HurtState = Animator.StringToHash("Hurt");
        private static readonly int DeathState = Animator.StringToHash("Death");

        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Color telegraphColor = new Color(1f, 0.75f, 0.35f);
        [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private float corpseTime = 1.2f;

        private KinematicMover mover;
        private EnemyBrain brain;
        private Health health;
        private EnemyState shownState;
        private float knockbackVelocity;
        private float fallVelocity;
        private float flashTimer;

        public EnemyState State => brain.State;

        public int CurrentHealth => health.Current;

        public bool TakeHit(in HitInfo hit)
        {
            if (health.IsDead)
            {
                return false;
            }

            flashTimer = HitFlashTime;
            bool killed = health.Damage(hit.Damage);
            knockbackVelocity = (transform.position.x >= hit.SourceX ? 1f : -1f) * hit.Knockback;

            if (killed)
            {
                brain.OnKilled();
                hitbox.Close();
                bodyCollider.enabled = false;
                StartCoroutine(RemoveCorpse());
            }
            else
            {
                brain.OnHit();
            }

            return true;
        }

        private void Awake()
        {
            mover = GetComponent<KinematicMover>();
            brain = new EnemyBrain(definition.ToBrainSettings());
            health = new Health(definition.MaxHealth);
            shownState = EnemyState.Patrol;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            mover.ProbeGround();

            Vector2 toTarget = target.position - transform.position;
            brain.Tick(new EnemySenses
            {
                ToTargetX = toTarget.x,
                ToTargetY = toTarget.y,
                BlockedAhead = IsBlockedAhead(),
                DeltaTime = dt,
            });

            fallVelocity = mover.Grounded ? 0f : fallVelocity - definition.Gravity * dt;
            knockbackVelocity = Mathf.MoveTowards(knockbackVelocity, 0f, definition.KnockbackDecay * dt);

            var velocity = new Vector2(brain.MoveX * definition.MoveSpeed + knockbackVelocity, fallVelocity);
            mover.Move(velocity, dt);
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = brain.Facing < 0;
            flashTimer = Mathf.Max(0f, flashTimer - Time.unscaledDeltaTime);

            if (flashTimer > 0f)
            {
                spriteRenderer.color = hitColor;
            }
            else
            {
                spriteRenderer.color = brain.State == EnemyState.Telegraph ? telegraphColor : Color.white;
            }

            if (brain.State != shownState)
            {
                EnterAnimation(brain.State);
            }
        }

        private void EnterAnimation(EnemyState state)
        {
            bool attackContinues = shownState == EnemyState.Telegraph && state == EnemyState.Strike;
            shownState = state;

            if (attackContinues)
            {
                return;
            }

            if (state == EnemyState.Telegraph)
            {
                hitbox.Configure(new HitInfo { Damage = definition.Damage, Knockback = definition.Knockback }, brain.Facing);
            }
            else
            {
                hitbox.Close();
            }

            animator.Play(AnimationFor(state), 0, 0f);
        }

        private static int AnimationFor(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Patrol:
                case EnemyState.Chase:
                    return WalkState;
                case EnemyState.Telegraph:
                case EnemyState.Strike:
                    return AttackState;
                case EnemyState.Hurt:
                    return HurtState;
                case EnemyState.Dead:
                    return DeathState;
                default:
                    return IdleState;
            }
        }

        private bool IsBlockedAhead()
        {
            var direction = new Vector2(brain.Facing, 0f);
            if (mover.IsBlocked(direction, LookAhead))
            {
                return true;
            }

            Bounds bounds = bodyCollider.bounds;
            var foot = new Vector2(bounds.center.x + brain.Facing * (bounds.extents.x + LookAhead), bounds.min.y + 0.05f);
            return mover.Grounded && !UnityEngine.Physics2D.Raycast(foot, Vector2.down, LedgeProbeDepth, mover.SolidLayers);
        }

        private IEnumerator RemoveCorpse()
        {
            yield return new WaitForSeconds(corpseTime);
            gameObject.SetActive(false);
        }
    }
}
