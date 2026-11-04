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
        private const float DropThroughStick = -0.5f;
        private const float DropThroughTime = 0.25f;

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
        private bool attacking;
        private bool attackBlocksJump;
        private float attackMoveScale = 1f;

        public bool IsInvulnerable => motor.IsInvulnerable;

        public bool IsDashing => motor.IsDashing;

        public bool IsStunned => motor.State == MotorState.Hurt;

        public int Facing => motor.Facing;

        /// <summary>Tells the motor how the current weapon limits movement while it swings.</summary>
        public void SetAttacking(bool attacking, float moveScale, bool blocksJump)
        {
            this.attacking = attacking;
            attackMoveScale = moveScale;
            attackBlocksJump = blocksJump;
        }

        /// <summary>Turns toward the held direction, so the next hit of a combo can change side.</summary>
        public void FaceInputDirection()
        {
            motor.Face(moveAction.action.ReadValue<Vector2>().x);
        }

        /// <summary>Pushes the player forward at the start of an attack.</summary>
        public void Lunge(float speed)
        {
            motor.Lunge(speed);
        }

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

        /// <summary>Plays the attack clip from a point in it, at the speed the step needs.</summary>
        public void PlayAttack(float normalizedStart, float speed)
        {
            shownState = AttackState;
            animator.speed = speed;
            animator.Play(AttackState, 0, normalizedStart);
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

            Vector2 stick = dead ? Vector2.zero : moveAction.action.ReadValue<Vector2>();
            if (jumpQueued && stick.y < DropThroughStick && mover.OnOneWayPlatform)
            {
                mover.DropThrough(DropThroughTime);
                jumpQueued = false;
            }

            motor.Step(new MotorInput
            {
                MoveX = stick.x,
                JumpPressed = jumpQueued && !dead,
                JumpHeld = jumpAction.action.IsPressed() && !dead,
                DashPressed = dashQueued && !dead,
                Attacking = attacking,
                AttackMoveScale = attackMoveScale,
                AttackBlocksJump = attackBlocksJump,
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
                animator.speed = 1f;
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

            if (attacking)
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
