using System;
using System.Collections;
using Ashbound.Gameplay.Enemies;
using Ashbound.Gameplay.Player;
using Ashbound.Logic;
using Ashbound.Logic.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Builds a run from a seed and restarts it on death or at the exit.</summary>
    public sealed class RunDirector : MonoBehaviour
    {
        private const ulong RoomStream = 1;
        private const ulong SpawnStream = 2;

        [SerializeField] private Room startRoom;
        [SerializeField] private Room exitRoom;
        [SerializeField] private Room[] roomPool;
        [SerializeField] private EnemyController[] enemyPool;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private int roomsPerRun = 3;
        [SerializeField, Range(0f, 1f)] private float spawnChance = 0.75f;
        [Tooltip("Plays a specific run. Zero picks a new seed every run.")]
        [SerializeField] private long fixedSeed;
        [SerializeField] private float restartDelay = 2f;

        private bool ending;

        /// <summary>Seed of the current run.</summary>
        public ulong Seed { get; private set; }

        /// <summary>Raised with a message when the run ends, before the restart.</summary>
        public event Action<string> RunEnded;

        private void Awake()
        {
            // Nine digits are short enough to read out and share.
            Seed = fixedSeed != 0 ? (ulong)fixedSeed : (ulong)(DateTime.Now.Ticks % 1000000000L);
            BuildRun();
        }

        private void OnEnable()
        {
            player.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            player.Died -= OnPlayerDied;
        }

        private void BuildRun()
        {
            var random = new SeededRandom(Seed);
            SeededRandom roomRandom = random.Fork(RoomStream);
            SeededRandom spawnRandom = random.Fork(SpawnStream);

            Room previous = Instantiate(startRoom, transform);
            previous.PlaceEntryAt(Vector3.zero);
            player.transform.position = previous.PlayerStart.position;

            foreach (int index in RoomChain.Build(roomRandom, roomPool.Length, roomsPerRun))
            {
                previous = PlaceAfter(roomPool[index], previous);
                SpawnEnemies(previous, spawnRandom);
            }

            Room last = PlaceAfter(exitRoom, previous);
            last.GetComponentInChildren<ExitGate>().Reached += OnExitReached;

            // Rooms were moved after they were created. Without this, the first physics step
            // still sees their colliders at the origin and everything starts falling.
            UnityEngine.Physics2D.SyncTransforms();
        }

        private Room PlaceAfter(Room prefab, Room previous)
        {
            Room room = Instantiate(prefab, transform);
            room.PlaceEntryAt(previous.Exit.position);
            return room;
        }

        private void SpawnEnemies(Room room, SeededRandom random)
        {
            bool[] used = RoomChain.PickSpawns(random, room.EnemySpawns.Length, spawnChance);
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i])
                {
                    EnemyController prefab = enemyPool[random.NextInt(0, enemyPool.Length)];
                    EnemyController enemy = Instantiate(prefab, room.EnemySpawns[i].position, Quaternion.identity, room.transform);
                    enemy.SetTarget(player.transform);
                }
            }
        }

        private void OnPlayerDied()
        {
            EndRun("You died");
        }

        private void OnExitReached()
        {
            EndRun("Run complete");
        }

        private void EndRun(string message)
        {
            if (ending)
            {
                return;
            }

            ending = true;
            RunEnded?.Invoke(message);
            StartCoroutine(Restart());
        }

        private IEnumerator Restart()
        {
            yield return new WaitForSeconds(restartDelay);
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}
