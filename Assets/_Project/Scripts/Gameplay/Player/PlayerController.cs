using Ashbound.Gameplay.Physics;
using Ashbound.Logic.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashbound.Gameplay.Player
{
    /// <summary>
    /// Binds input and the kinematic body to <see cref="PlayerMotor"/>, and plays the animation
    /// for the current state.
    /// </summary>
    [RequireComponent(typeof(KinematicMover))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float BlinkRate = 16f;

        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int RunState = Animator.StringToHash("Run");
        private static readonly int JumpState = Animator.StringToHash("Jump");
        private static readonly int FallState = Animator.StringToHash("Fall");
        private static readonly int DashState = Animator.StringToHash("Dash");
        private static readonly int HurtState = Animator.StringToHash("Hurt");
        private static readonly int AttackState = Animator.StringToHash("Attack");
        private static readonly int DeathState = Animator.StringToHash("Death");

        [SerializeField] private PlayerMovementTuning tuning;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private KinematicMover mover;
        private PlayerMotor motor;
        private int shownState;
        private bool jumpQueued;
        private bool dashQueued;
        private bool dead;

        public bool IsInvulnerable => motor.IsInvulnerable;

        public bool IsDashing => motor.IsDashing;

        public bool IsStunned => motor.State == MotorState.Hurt;

        public int Facing => motor.Facing;

        /// <summary>Set by the combat component while an attack is playing.</summary>
        public bool Attacking { get; set; }

        /// <summary>Applies the movement side of a hit.</summary>
        public void Hurt(float knockbackX, float knockbackY, float stunTime, float invulnerability)
        {
            motor.Hurt(knockbackX, knockbackY, stunTime, invulnerability);
        }

        /// <summary>Stops all control and plays the death animation.</summary>
        public void Die()
        {
            dead = true;
        }

        /// <summary>Restarts the attack animation for a chained attack.</summary>
        public void RestartAttackAnimation()
        {
            shownState = AttackState;
            animator.Play(AttackState, 0, 0f);
        }

        private void Awake()
        {
            mover = GetComponent<KinematicMover>();
            motor = new PlayerMotor(tuning.ToSettings());
            shownState = IdleState;
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            dashAction.action.Enable();
        }

        private void Update()
        {
            // Presses are read per frame, because FixedUpdate can skip a frame and lose them.
            jumpQueued |= jumpAction.action.WasPressedThisFrame();
            dashQueued |= dashAction.action.WasPressedThisFrame();
        }

        private void FixedUpdate()
        {
            mover.ProbeGround();

            motor.Step(new MotorInput
            {
                MoveX = dead ? 0f : moveAction.action.ReadValue<Vector2>().x,
                JumpPressed = jumpQueued && !dead,
                JumpHeld = jumpAction.action.IsPressed() && !dead,
                DashPressed = dashQueued && !dead,
                Attacking = Attacking,
                Grounded = mover.Grounded,
                HitCeiling = mover.HitCeiling,
                DeltaTime = Time.fixedDeltaTime,
            });

            jumpQueued = false;
            dashQueued = false;

            mover.Move(new Vector2(motor.VelocityX, motor.VelocityY), Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = motor.Facing < 0;

            bool blink = !dead && motor.IsInvulnerable && !motor.IsDashing
                && Mathf.Repeat(Time.time * BlinkRate, 2f) < 1f;
            spriteRenderer.color = blink ? new Color(1f, 1f, 1f, 0.35f) : Color.white;

            int wanted = PickAnimation();
            if (wanted != shownState)
            {
                shownState = wanted;
                animator.Play(wanted, 0, 0f);
            }
        }

        private int PickAnimation()
        {
            if (dead)
            {
                return DeathState;
            }

            if (motor.State == MotorState.Hurt)
            {
                return HurtState;
            }

            if (motor.State == MotorState.Dash)
            {
                return DashState;
            }

            if (Attacking)
            {
                return AttackState;
            }

            switch (motor.State)
            {
                case MotorState.Run:
                    return RunState;
                case MotorState.Jump:
                    return JumpState;
                case MotorState.Fall:
                    return FallState;
                default:
                    return IdleState;
            }
        }
    }
}
