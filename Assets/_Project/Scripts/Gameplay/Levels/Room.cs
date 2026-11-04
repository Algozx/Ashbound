using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>A hand-made room. Rooms connect by placing one's entry on the previous one's exit.</summary>
    public sealed class Room : MonoBehaviour
    {
        [SerializeField] private Transform entry;
        [SerializeField] private Transform exit;
        [SerializeField] private Transform playerStart;
        [SerializeField] private Transform[] enemySpawns;

        public Transform Entry => entry;

        public Transform Exit => exit;

        public Transform PlayerStart => playerStart;

        public Transform[] EnemySpawns => enemySpawns;

        /// <summary>Moves the room so its entry sits on a world position.</summary>
        public void PlaceEntryAt(Vector3 position)
        {
            transform.position += position - entry.position;
        }
    }
}
