using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>
    /// Attack area opened and closed by animation events. Each target is hit once per opening.
    /// </summary>
    public sealed class Hitbox : MonoBehaviour
    {
        private const int MaxTargets = 8;

        [SerializeField] private Vector2 size = new Vector2(2f, 1.6f);
        [SerializeField] private LayerMask targetLayers;

        private readonly Collider2D[] results = new Collider2D[MaxTargets];
        private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
        private ContactFilter2D filter;
        private HitInfo hit;
        private float baseOffsetX;
        private bool open;

        /// <summary>Raised for every hit that lands.</summary>
        public event Action HitLanded;

        /// <summary>Sets what the next opening delivers and which side it faces.</summary>
        public void Configure(in HitInfo nextHit, int facing)
        {
            hit = nextHit;
            transform.localPosition = new Vector3(baseOffsetX * facing, transform.localPosition.y, 0f);
        }

        public void Open()
        {
            alreadyHit.Clear();
            open = true;
        }

        public void Close()
        {
            open = false;
        }

        private void Awake()
        {
            baseOffsetX = transform.localPosition.x;
            filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = targetLayers };
        }

        private void FixedUpdate()
        {
            if (!open)
            {
                return;
            }

            hit.SourceX = transform.parent.position.x;
            int count = UnityEngine.Physics2D.OverlapBox(transform.position, size, 0f, filter, results);
            for (int i = 0; i < count; i++)
            {
                var target = results[i].GetComponentInParent<IDamageable>();
                if (target != null && alreadyHit.Add(target) && target.TakeHit(hit))
                {
                    HitLanded?.Invoke();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = open ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
