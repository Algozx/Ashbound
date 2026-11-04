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
        private const ulong LootStream = 3;
        private const float RoomMargin = 2.5f;

        [SerializeField] private Room startRoom;
        [SerializeField] private Room exitRoom;
        [SerializeField] private Room[] roomPool;
        [SerializeField] private EnemyController[] enemyPool;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private int roomsPerRun = 3;
        [SerializeField] private LootEntry[] loot;
        [Header("Difficulty")]
        [Tooltip("Extra enemy health and damage per room, as a share of the base value.")]
        [SerializeField] private float statStepPerRoom = 0.3f;
        [SerializeField, Range(0f, 1f)] private float baseSpawnChance = 0.6f;
        [SerializeField, Range(0f, 1f)] private float spawnChanceStep = 0.2f;

        [Header("Seed")]
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
            SeededRandom lootRandom = random.Fork(LootStream);

            Room previous = Instantiate(startRoom, transform);
            previous.PlaceEntryAt(Vector3.zero);
            player.transform.position = previous.PlayerStart.position;

            var difficulty = new DifficultyCurve(statStepPerRoom, baseSpawnChance, spawnChanceStep);
            int[] chain = RoomChain.Build(roomRandom, roomPool.Length, roomsPerRun);
            for (int depth = 0; depth < chain.Length; depth++)
            {
                previous = PlaceAfter(roomPool[chain[depth]], previous);
                SpawnEnemies(previous, spawnRandom, difficulty, depth);
                SpawnLoot(previous, lootRandom);
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

        private void SpawnEnemies(Room room, SeededRandom random, DifficultyCurve difficulty, int depth)
        {
            bool[] used = RoomChain.PickSpawns(random, room.EnemySpawns.Length, difficulty.SpawnChance(depth));
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i])
                {
                    EnemyController prefab = enemyPool[random.NextInt(0, enemyPool.Length)];
                    EnemyController enemy = Instantiate(prefab, room.EnemySpawns[i].position, Quaternion.identity, room.transform);
                    enemy.Configure(player.transform, room.Entry.position.x + RoomMargin, room.Exit.position.x - RoomMargin, difficulty.StatScale(depth));
                }
            }
        }

        private void SpawnLoot(Room room, SeededRandom random)
        {
            var weights = new int[loot.Length];
            for (int i = 0; i < loot.Length; i++)
            {
                weights[i] = loot[i].Weight;
            }

            GameObject prefab = loot[LootTable.Pick(random, weights)].Prefab;
            Instantiate(prefab, room.LootSpawn.position, Quaternion.identity, room.transform);
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
