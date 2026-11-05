using System.Collections;
using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Physics;
using Ashbound.Logic;
using Ashbound.Logic.Combat;
using Ashbound.Logic.Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>Binds <see cref="EnemyBrain"/> to a body, an animator and its attacks.</summary>
    [RequireComponent(typeof(KinematicMover))]
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        private const float HitFlashTime = 0.1f;
        private const float LookAhead = 0.1f;
        private const float LedgeProbeDepth = 0.6f;
        private const float TargetChestHeight = 1f;
        private const float ClimbMinHeight = 1f;
        private const float ClimbClearance = 0.5f;
        private const float DetourStep = 0.5f;
        private const float DetourSearch = 8f;

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
        [SerializeField] private AttackAnimationEvents animationEvents;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Color telegraphColor = new Color(1f, 0.75f, 0.35f);
        [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private float corpseTime = 1.2f;

        private KinematicMover mover;
        private EnemyBrain brain;
        private Health health;
        private ObjectPool<Projectile> projectiles;
        private EnemyState shownState;
        private int damage;
        private int embers;
        private float knockbackVelocity;
        private float fallVelocity;
        private float flashTimer;
        private float minX = float.NegativeInfinity;
        private float maxX = float.PositiveInfinity;

        public EnemyState State => brain.State;

        public int CurrentHealth => health.Current;

        /// <summary>Raised on the killing hit, with the spot and the Embers this enemy drops.</summary>
        public event System.Action<Vector2, int> Killed;

        /// <summary>Prepares a spawned enemy: who it hunts, where it stays and how strong it is.</summary>
        public void Configure(Transform newTarget, float roomMinX, float roomMaxX, float statScale, float emberScale, SeededRandom patrolRandom)
        {
            brain.UseRandom(patrolRandom);
            target = newTarget;
            minX = roomMinX;
            maxX = roomMaxX;
            health = new Health(Mathf.RoundToInt(definition.MaxHealth * statScale));
            damage = Mathf.RoundToInt(definition.Damage * statScale);
            embers = Mathf.RoundToInt(definition.Embers * statScale * emberScale);
        }

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
                Killed?.Invoke(transform.position, embers);
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
            damage = definition.Damage;
            embers = definition.Embers;
            shownState = EnemyState.Patrol;

            if (definition.Projectile != null)
            {
                projectiles = new ObjectPool<Projectile>(
                    () => Instantiate(definition.Projectile, transform.parent),
                    shot => shot.gameObject.SetActive(true),
                    shot => shot.gameObject.SetActive(false));
            }
        }

        private void OnEnable()
        {
            animationEvents.ProjectileRequested += FireProjectile;
        }

        private void OnDisable()
        {
            animationEvents.ProjectileRequested -= FireProjectile;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            mover.ProbeGround();

            Vector2 toTarget = target.position - transform.position;
            bool roomEdge = IsAtRoomEdge();
            bool wall = mover.IsBlocked(new Vector2(brain.Facing, 0f), LookAhead);
            bool ledge = IsAtLedge();
            brain.Tick(new EnemySenses
            {
                X = transform.position.x,
                Y = transform.position.y,
                ToTargetX = toTarget.x,
                ToTargetY = toTarget.y,
                BlockedAhead = roomEdge || wall || ledge,
                WallAhead = wall,
                LedgeAhead = ledge,
                RoomEdgeAhead = roomEdge,
                Grounded = mover.Grounded,
                TargetOutsideTerritory = target.position.x < minX || target.position.x > maxX,
                ClimbDetour = FindClimbDetour(toTarget),
                DeltaTime = dt,
            });

            if (brain.ConsumeJump())
            {
                fallVelocity = definition.JumpSpeed;
            }

            knockbackVelocity = Mathf.MoveTowards(knockbackVelocity, 0f, definition.KnockbackDecay * dt);
            mover.Move(new Vector2(brain.MoveX * definition.MoveSpeed + knockbackVelocity, VerticalVelocity(dt)), dt);
        }

        private float VerticalVelocity(float dt)
        {
            bool stillFlying = definition.Flies && brain.State != EnemyState.Dead;
            if (stillFlying)
            {
                return brain.MoveY * definition.MoveSpeed;
            }

            if (mover.HitCeiling && fallVelocity > 0f)
            {
                fallVelocity = 0f;
            }

            bool landed = mover.Grounded && fallVelocity <= 0f;
            fallVelocity = landed ? 0f : fallVelocity - definition.Gravity * dt;
            return fallVelocity;
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
                hitbox.Configure(new HitInfo { Damage = damage, Knockback = definition.Knockback }, brain.Facing);
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
                case EnemyState.Return:
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

        private void FireProjectile()
        {
            Vector2 offset = definition.MuzzleOffset;
            var muzzle = (Vector2)transform.position + new Vector2(offset.x * brain.Facing, offset.y);
            Vector2 aim = (Vector2)target.position + Vector2.up * TargetChestHeight;
            var hit = new HitInfo { Damage = damage, Knockback = definition.Knockback };
            projectiles.Get().Launch(muzzle, aim - muzzle, hit, projectiles.Release);
        }

        private bool IsAtRoomEdge()
        {
            float x = transform.position.x;
            return (brain.Facing > 0 && x >= maxX) || (brain.Facing < 0 && x <= minX);
        }

        private bool IsAtLedge()
        {
            if (definition.Flies || !mover.Grounded)
            {
                return false;
            }

            Bounds bounds = bodyCollider.bounds;
            var foot = new Vector2(bounds.center.x + brain.Facing * (bounds.extents.x + LookAhead), bounds.min.y + 0.05f);
            return !UnityEngine.Physics2D.Raycast(foot, Vector2.down, LedgeProbeDepth, mover.GroundLayers);
        }

        // Platforms are only entered at their ends, so an enemy under one walks to the nearest free
        // column before it jumps. Returns the side to walk to, or zero when the way up is free.
        private int FindClimbDetour(Vector2 toTarget)
        {
            if (!definition.CanJump || !mover.Grounded || toTarget.y < ClimbMinHeight)
            {
                return 0;
            }

            float x = transform.position.x;
            if (IsColumnFree(x))
            {
                return 0;
            }

            for (float offset = DetourStep; offset <= DetourSearch; offset += DetourStep)
            {
                bool rightFree = IsColumnFree(x + offset);
                bool leftFree = IsColumnFree(x - offset);
                if (rightFree && leftFree)
                {
                    return toTarget.x >= 0f ? 1 : -1;
                }

                if (rightFree || leftFree)
                {
                    return rightFree ? 1 : -1;
                }
            }

            return 0;
        }

        private bool IsColumnFree(float x)
        {
            Bounds bounds = bodyCollider.bounds;
            float rise = definition.JumpHeight;
            var center = new Vector2(x, bounds.max.y + rise * 0.5f);
            var size = new Vector2(bounds.size.x + ClimbClearance * 2f, rise);
            return UnityEngine.Physics2D.OverlapBox(center, size, 0f, mover.PlatformCoreLayers) == null;
        }

        private IEnumerator RemoveCorpse()
        {
            yield return new WaitForSeconds(corpseTime);
            gameObject.SetActive(false);
        }
    }
}
