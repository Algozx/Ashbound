using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>A small hand-made room: the hub, a rest room or the boss arena.</summary>
    public sealed class Room : MonoBehaviour
    {
        [SerializeField] private Transform entry;
        [SerializeField] private Transform exit;
        [SerializeField] private Transform playerStart;

        public Transform Entry => entry;

        public Transform Exit => exit;

        public Transform PlayerStart => playerStart;

        /// <summary>Moves the room so its entry sits on a world position.</summary>
        public void PlaceEntryAt(Vector3 position)
        {
            transform.position += position - entry.position;
        }
    }
}
