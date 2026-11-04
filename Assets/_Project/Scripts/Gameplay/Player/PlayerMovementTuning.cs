using Ashbound.Logic.Movement;
using UnityEngine;

namespace Ashbound.Gameplay.Player
{
    /// <summary>Movement values edited in the Inspector. Speeds are in units per second.</summary>
    [CreateAssetMenu(menuName = "Ashbound/Player Movement Tuning", fileName = "PlayerMovementTuning")]
    public sealed class PlayerMovementTuning : ScriptableObject
    {
        [Header("Run")]
        [SerializeField] private float runSpeed = 9f;
        [SerializeField] private float groundAcceleration = 90f;
        [SerializeField] private float airAcceleration = 55f;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 4.5f;
        [SerializeField] private float timeToApex = 0.38f;
        [SerializeField] private float fallGravityMultiplier = 1.6f;
        [SerializeField] private float maxFallSpeed = 24f;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 22f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.55f;
        [SerializeField] private float dashInvulnerability = 0.22f;

        [Header("Attack")]
        [SerializeField, Range(0f, 1f)] private float attackMoveScale = 0.15f;

        public PlayerMotorSettings ToSettings()
        {
            return new PlayerMotorSettings
            {
                RunSpeed = runSpeed,
                GroundAcceleration = groundAcceleration,
                AirAcceleration = airAcceleration,
                JumpHeight = jumpHeight,
                TimeToApex = timeToApex,
                FallGravityMultiplier = fallGravityMultiplier,
                MaxFallSpeed = maxFallSpeed,
                JumpCutMultiplier = jumpCutMultiplier,
                CoyoteTime = coyoteTime,
                JumpBufferTime = jumpBufferTime,
                DashSpeed = dashSpeed,
                DashDuration = dashDuration,
                DashCooldown = dashCooldown,
                DashInvulnerability = dashInvulnerability,
                AttackMoveScale = attackMoveScale,
            };
        }
    }
}
