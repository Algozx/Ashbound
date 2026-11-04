using UnityEngine;

namespace Ashbound.Gameplay.Physics
{
    /// <summary>Moves a kinematic body and stops it against solid layers.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class KinematicMover : MonoBehaviour
    {
        private const float SkinWidth = 0.02f;
        private const int MaxHits = 8;

        [SerializeField] private LayerMask solidLayers;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[MaxHits];
        private Rigidbody2D body;
        private ContactFilter2D solidFilter;

        public bool Grounded { get; private set; }

        public bool HitCeiling { get; private set; }

        public LayerMask SolidLayers => solidLayers;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            solidFilter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = solidLayers };
        }

        /// <summary>Refreshes <see cref="Grounded"/>. Call it before reading the state of this step.</summary>
        public void ProbeGround()
        {
            Grounded = body.Cast(Vector2.down, solidFilter, hits, SkinWidth * 2f) > 0;
        }

        /// <summary>True when something solid is within distance along direction.</summary>
        public bool IsBlocked(Vector2 direction, float distance)
        {
            return body.Cast(direction, solidFilter, hits, distance) > 0;
        }

        public void Move(Vector2 velocity, float deltaTime)
        {
            Vector2 delta = velocity * deltaTime;
            float moveX = Sweep(Vector2.right * Mathf.Sign(delta.x), Mathf.Abs(delta.x));
            float moveY = Sweep(Vector2.up * Mathf.Sign(delta.y), Mathf.Abs(delta.y));
            HitCeiling = delta.y > 0f && moveY < delta.y;

            body.MovePosition(body.position + new Vector2(moveX * Mathf.Sign(delta.x), moveY * Mathf.Sign(delta.y)));
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
    }
}
