using System;
using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Physics;
using Ashbound.Logic.Combat;
using Ashbound.Logic.Enemies;
using UnityEngine;
using UnityEngine.Pool;

namespace Ashbound.Gameplay.Enemies
{
    /// <summary>Binds <see cref="BossBrain"/> to a body, an animator and its attacks.</summary>
    [RequireComponent(typeof(KinematicMover))]
    public sealed class BossController : MonoBehaviour, IDamageable
    {
        private const float HitFlashTime = 0.08f;
        private const float GroundProbeDistance = 30f;
        private const float Gravity = 40f;

        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int AttackState = Animator.StringToHash("Attack");
        private static readonly int CastState = Animator.StringToHash("Cast");
        private static readonly int DeathState = Animator.StringToHash("Death");

        [SerializeField] private BossDefinition definition;
        [SerializeField] private BossSpell spellPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private AttackAnimationEvents animationEvents;
        [SerializeField] private Color enragedColor = new Color(1f, 0.7f, 0.7f);
        [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f);

        private KinematicMover mover;
        private BossBrain brain;
        private Health health;
        private ObjectPool<BossSpell> spells;
        private Transform target;
        private BossState shownState;
        private float fallVelocity;
        private float flashTimer;

        public string DisplayName => definition.DisplayName;

        public int CurrentHealth => health.Current;

        public int MaxHealth => health.Max;

        public BossState State => brain.State;

        public bool IsEnraged => brain.IsEnraged;

        /// <summary>Raised with current and maximum health after a hit.</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once, when the boss dies.</summary>
        public event Action Defeated;

        /// <summary>Starts the fight against a target.</summary>
        public void Wake(Transform newTarget)
        {
            target = newTarget;
            brain.Wake();
        }

        public bool TakeHit(in HitInfo hit)
        {
            if (health.IsDead || brain.State == BossState.Waiting)
            {
                return false;
            }

            flashTimer = HitFlashTime;
            bool killed = health.Damage(hit.Damage);
            brain.OnHealthChanged((float)health.Current / health.Max);
            HealthChanged?.Invoke(health.Current, health.Max);

            if (killed)
            {
                brain.OnKilled();
                hitbox.Close();
                Defeated?.Invoke();
            }

            return true;
        }

        private void Awake()
        {
            mover = GetComponent<KinematicMover>();
            brain = new BossBrain(definition.ToBrainSettings());
            health = new Health(definition.MaxHealth);
            shownState = BossState.Waiting;
            spells = new ObjectPool<BossSpell>(
                () => Instantiate(spellPrefab, transform.parent),
                spell => spell.gameObject.SetActive(true),
                spell => spell.gameObject.SetActive(false));
        }

        private void OnEnable()
        {
            animationEvents.ProjectileRequested += CastSpells;
        }

        private void OnDisable()
        {
            animationEvents.ProjectileRequested -= CastSpells;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            mover.ProbeGround();

            float toTargetX = target != null ? target.position.x - transform.position.x : 0f;
            brain.Tick(toTargetX, dt);

            fallVelocity = mover.Grounded ? 0f : fallVelocity - Gravity * dt;
            mover.Move(new Vector2(brain.MoveX * definition.MoveSpeed, fallVelocity), dt);
        }

        private void LateUpdate()
        {
            // The sprites face left.
            spriteRenderer.flipX = brain.Facing > 0;
            flashTimer = Mathf.Max(0f, flashTimer - Time.unscaledDeltaTime);

            if (flashTimer > 0f)
            {
                spriteRenderer.color = hitColor;
            }
            else
            {
                spriteRenderer.color = brain.IsEnraged ? enragedColor : Color.white;
            }

            if (brain.State != shownState)
            {
                EnterAnimation(brain.State);
            }
        }

        private void EnterAnimation(BossState state)
        {
            BossState previous = shownState;
            shownState = state;

            bool attackContinues = previous == BossState.MeleeWindup && state == BossState.MeleeStrike;
            bool castContinues = previous == BossState.CastWindup && state == BossState.CastRelease;
            if (attackContinues || castContinues)
            {
                return;
            }

            if (state == BossState.MeleeWindup)
            {
                hitbox.Configure(new HitInfo { Damage = definition.MeleeDamage, Knockback = definition.Knockback }, brain.Facing);
            }
            else
            {
                hitbox.Close();
            }

            animator.Play(AnimationFor(state), 0, 0f);
        }

        private static int AnimationFor(BossState state)
        {
            switch (state)
            {
                case BossState.Approach:
                    return WalkState;
                case BossState.MeleeWindup:
                case BossState.MeleeStrike:
                    return AttackState;
                case BossState.CastWindup:
                case BossState.CastRelease:
                    return CastState;
                case BossState.Dead:
                    return DeathState;
                default:
                    return IdleState;
            }
        }

        private void CastSpells()
        {
            var hit = new HitInfo { Damage = definition.SpellDamage, Knockback = definition.Knockback };
            int count = brain.SpellCount;
            for (int i = 0; i < count; i++)
            {
                // Spreads out from the player: 0, +1, -1, +2...
                int step = (i + 1) / 2 * (i % 2 == 0 ? -1 : 1);
                float x = target.position.x + step * definition.SpellSpacing;
                spells.Get().Launch(GroundBelow(x), hit, spells.Release);
            }
        }

        private Vector2 GroundBelow(float x)
        {
            var from = new Vector2(x, target.position.y + 1f);
            RaycastHit2D ground = UnityEngine.Physics2D.Raycast(from, Vector2.down, GroundProbeDistance, mover.GroundLayers);
            return ground.collider != null ? ground.point : from;
        }
    }
}
