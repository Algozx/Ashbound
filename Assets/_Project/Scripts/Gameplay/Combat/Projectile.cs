using System;
using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>A shot that flies straight until it hits a target or a wall.</summary>
    public sealed class Projectile : MonoBehaviour
    {
        private const int MaxTargets = 4;

        [SerializeField] private float speed = 7f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float radius = 0.3f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask solidLayers;

        private readonly Collider2D[] results = new Collider2D[MaxTargets];
        private ContactFilter2D targetFilter;
        private Action<Projectile> release;
        private Vector2 velocity;
        private HitInfo hit;
        private float age;

        /// <summary>Starts a pooled shot. The callback returns it to the pool.</summary>
        public void Launch(Vector2 position, Vector2 direction, in HitInfo shotHit, Action<Projectile> onFinished)
        {
            transform.position = position;
            velocity = direction.normalized * speed;
            hit = shotHit;
            release = onFinished;
            age = 0f;
        }

        private void Awake()
        {
            targetFilter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = targetLayers };
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            transform.position += (Vector3)(velocity * dt);
            age += dt;

            bool hitWall = UnityEngine.Physics2D.OverlapCircle(transform.position, radius, solidLayers) != null;
            if (hitWall || age >= lifetime || HitTarget())
            {
                release(this);
            }
        }

        private bool HitTarget()
        {
            hit.SourceX = transform.position.x - velocity.x;
            int count = UnityEngine.Physics2D.OverlapCircle(transform.position, radius, targetFilter, results);
            for (int i = 0; i < count; i++)
            {
                var target = results[i].GetComponentInParent<IDamageable>();
                if (target != null && target.TakeHit(hit))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
