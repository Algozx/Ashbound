using System;
using UnityEngine;

namespace Ashbound.Gameplay.Meta
{
    /// <summary>An Ember that bursts out of a kill and then flies to the player.</summary>
    public sealed class EmberPickup : MonoBehaviour
    {
        private const float ChestHeight = 1f;

        [SerializeField] private float burstSpeed = 6f;
        [SerializeField] private float burstTime = 0.35f;
        [SerializeField] private float homingSpeed = 16f;
        [SerializeField] private float collectDistance = 0.4f;

        private Transform target;
        private Action<EmberPickup> collected;
        private Vector2 velocity;
        private float age;

        public int Value { get; private set; }

        /// <summary>Starts a pooled Ember. Its burst direction is only cosmetic.</summary>
        public void Launch(Vector2 position, int value, Transform player, Action<EmberPickup> onCollected)
        {
            transform.position = position;
            Value = value;
            target = player;
            collected = onCollected;
            age = 0f;
            var direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.4f, 1f));
            velocity = direction.normalized * burstSpeed;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age < burstTime)
            {
                transform.position += (Vector3)(velocity * (1f - age / burstTime) * Time.deltaTime);
                return;
            }

            Vector2 chest = (Vector2)target.position + Vector2.up * ChestHeight;
            transform.position = Vector2.MoveTowards(transform.position, chest, homingSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, chest) <= collectDistance)
            {
                collected(this);
            }
        }
    }
}
