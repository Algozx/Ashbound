using UnityEngine;

namespace Ashbound.Gameplay.Physics
{
    /// <summary>
    /// Moves a kinematic body and stops it against solid layers. One-way platforms only stop a body
    /// that lands on them from above.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class KinematicMover : MonoBehaviour
    {
        private const float SkinWidth = 0.02f;
        private const float FacingSurface = 0.5f;
        private const float LandingTolerance = 0.05f;
        private const int MaxHits = 8;

        [SerializeField] private LayerMask solidLayers;
        [SerializeField] private LayerMask oneWayLayers;
        [Tooltip("Platform middles: bodies land on them and drop through them, but cannot rise through them.")]
        [SerializeField] private LayerMask platformCoreLayers;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[MaxHits];
        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private ContactFilter2D filter;
        private float groundGap;
        private float dropTimer;

        public bool Grounded { get; private set; }

        public bool HitCeiling { get; private set; }

        /// <summary>True while the body stands on a platform it could drop through.</summary>
        public bool OnOneWayPlatform { get; private set; }

        /// <summary>Everything a body can stand on.</summary>
        public LayerMask GroundLayers => solidLayers | oneWayLayers | platformCoreLayers;

        /// <summary>Platform parts that stop this body from rising.</summary>
        public LayerMask PlatformCoreLayers => platformCoreLayers;

        /// <summary>Lets the body fall through one-way platforms for a moment.</summary>
        public void DropThrough(float seconds)
        {
            dropTimer = seconds;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = GroundLayers };
        }

        /// <summary>Refreshes <see cref="Grounded"/>. Call it before reading the state of this step.</summary>
        public void ProbeGround()
        {
            dropTimer = Mathf.Max(0f, dropTimer - Time.fixedDeltaTime);
            groundGap = NearestSurface(Vector2.down, SkinWidth * 2f, out bool oneWay);
            Grounded = groundGap <= SkinWidth * 2f;
            OnOneWayPlatform = Grounded && oneWay;
        }

        /// <summary>True when a surface facing the body is within distance along direction.</summary>
        public bool IsBlocked(Vector2 direction, float distance)
        {
            return NearestSurface(direction, distance, out _) <= distance;
        }

        public void Move(Vector2 velocity, float deltaTime)
        {
            Vector2 delta = velocity * deltaTime;
            float moveX = Sweep(Vector2.right * Mathf.Sign(delta.x), Mathf.Abs(delta.x));
            float moveY = Sweep(Vector2.up * Mathf.Sign(delta.y), Mathf.Abs(delta.y));
            HitCeiling = delta.y > 0f && moveY < delta.y;

            float signedY = moveY * Mathf.Sign(delta.y);
            if (Grounded && delta.y <= 0f && groundGap < SkinWidth)
            {
                // Keeps the body off the floor, so walking never scrapes it.
                signedY = SkinWidth - groundGap;
            }

            body.MovePosition(body.position + new Vector2(moveX * Mathf.Sign(delta.x), signedY));
        }

        // Distance the body can travel along direction before touching something solid.
        private float Sweep(Vector2 direction, float distance)
        {
            if (distance <= 0f)
            {
                return 0f;
            }

            float nearest = NearestSurface(direction, distance + SkinWidth, out _);
            return Mathf.Min(distance, Mathf.Max(0f, nearest - SkinWidth));
        }

        // Only surfaces that face the movement count, so a floor never stops a sideways step.
        private float NearestSurface(Vector2 direction, float distance, out bool oneWay)
        {
            int count = body.Cast(direction, filter, hits, distance);
            float nearest = float.PositiveInfinity;
            oneWay = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = hits[i];
                int layerBit = 1 << hit.collider.gameObject.layer;
                bool hitCore = (platformCoreLayers.value & layerBit) != 0;
                bool hitOneWay = hitCore || (oneWayLayers.value & layerBit) != 0;
                bool stops = !hitOneWay || LandsOn(hit, direction) || (hitCore && BumpsInto(hit, direction));
                if (Vector2.Dot(hit.normal, -direction) <= FacingSurface || !stops)
                {
                    continue;
                }

                if (hit.distance < nearest)
                {
                    nearest = hit.distance;
                    oneWay = hitOneWay;
                }
            }

            return nearest;
        }

        private bool LandsOn(in RaycastHit2D hit, Vector2 direction)
        {
            bool fromAbove = bodyCollider.bounds.min.y >= hit.point.y - LandingTolerance;
            return direction.y < 0f && fromAbove && dropTimer <= 0f;
        }

        // A body whose head is already past the underside keeps rising, so a platform corner never
        // catches a jump that started at the open end.
        private bool BumpsInto(in RaycastHit2D hit, Vector2 direction)
        {
            bool fromBelow = bodyCollider.bounds.max.y <= hit.point.y + LandingTolerance;
            return direction.y > 0f && hit.distance > 0f && fromBelow;
        }
    }
}
