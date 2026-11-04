using Ashbound.Logic.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashbound.Gameplay.Player
{
    /// <summary>
    /// Binds input and the kinematic body to <see cref="PlayerMotor"/>, and plays the animation
    /// for the current motor state.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float SkinWidth = 0.02f;
        private const int MaxHits = 8;

        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int RunState = Animator.StringToHash("Run");
        private static readonly int JumpState = Animator.StringToHash("Jump");
        private static readonly int FallState = Animator.StringToHash("Fall");
        private static readonly int DashState = Animator.StringToHash("Dash");

        [SerializeField] private PlayerMovementTuning tuning;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private LayerMask solidLayers;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[MaxHits];
        private Rigidbody2D body;
        private ContactFilter2D solidFilter;
        private PlayerMotor motor;
        private MotorState shownState;
        private bool jumpQueued;
        private bool dashQueued;
        private bool grounded;
        private bool hitCeiling;

        public bool IsInvulnerable => motor.IsInvulnerable;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            motor = new PlayerMotor(tuning.ToSettings());
            solidFilter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = solidLayers };
            shownState = MotorState.Idle;
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
            grounded = Probe(Vector2.down);

            motor.Step(new MotorInput
            {
                MoveX = moveAction.action.ReadValue<Vector2>().x,
                JumpPressed = jumpQueued,
                JumpHeld = jumpAction.action.IsPressed(),
                DashPressed = dashQueued,
                Grounded = grounded,
                HitCeiling = hitCeiling,
                DeltaTime = Time.fixedDeltaTime,
            });

            jumpQueued = false;
            dashQueued = false;

            Vector2 delta = new Vector2(motor.VelocityX, motor.VelocityY) * Time.fixedDeltaTime;
            float moveX = Sweep(Vector2.right * Mathf.Sign(delta.x), Mathf.Abs(delta.x));
            float moveY = Sweep(Vector2.up * Mathf.Sign(delta.y), Mathf.Abs(delta.y));
            hitCeiling = delta.y > 0f && moveY < delta.y;

            body.MovePosition(body.position + new Vector2(moveX * Mathf.Sign(delta.x), moveY * Mathf.Sign(delta.y)));
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = motor.Facing < 0;

            if (motor.State != shownState)
            {
                shownState = motor.State;
                animator.Play(StateHash(shownState));
            }
        }

        private bool Probe(Vector2 direction)
        {
            return body.Cast(direction, solidFilter, hits, SkinWidth * 2f) > 0;
        }

        // Distance the body can travel along direction before touching something solid.
        private float Sweep(Vector2 direction, float distance)
        {
            if (distance <= 0f)
            {
                return 0f;
            }

            int count = body.Cast(direction, solidFilter, hits, distance + SkinWidth);
            float allowed = distance;
            for (int i = 0; i < count; i++)
            {
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hits[i].distance - SkinWidth));
            }

            return allowed;
        }

        private static int StateHash(MotorState state)
        {
            switch (state)
            {
                case MotorState.Run:
                    return RunState;
                case MotorState.Jump:
                    return JumpState;
                case MotorState.Fall:
                    return FallState;
                case MotorState.Dash:
                    return DashState;
                default:
                    return IdleState;
            }
        }
    }
}
