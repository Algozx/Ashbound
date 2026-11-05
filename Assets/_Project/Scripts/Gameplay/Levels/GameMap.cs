using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>A large hand-made map. Enemies and treasure are chosen by the run's seed.</summary>
    public sealed class GameMap : MonoBehaviour
    {
        [SerializeField] private Tilemap solids;
        [SerializeField] private Transform entry;
        [SerializeField] private Transform[] enemySpawns;
        [SerializeField] private Transform[] treasureSpots;
        [SerializeField] private Portal safeExit;
        [SerializeField] private Portal dangerExit;

        public Tilemap Solids => solids;

        public Transform Entry => entry;

        public Transform[] EnemySpawns => enemySpawns;

        public Transform[] TreasureSpots => treasureSpots;

        public Portal SafeExit => safeExit;

        public Portal DangerExit => dangerExit;
    }
}
