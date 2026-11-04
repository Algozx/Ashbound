using System;
using System.Collections;
using Ashbound.Gameplay.Enemies;
using Ashbound.Gameplay.Meta;
using Ashbound.Gameplay.Player;
using Ashbound.Logic;
using Ashbound.Logic.Levels;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>
    /// Builds a run from a seed: the hub, then maps of rooms with a rest room after each one, then the
    /// boss. Portals join the parts, and the run goes back to the hub on death or at the exit.
    /// </summary>
    public sealed class RunDirector : MonoBehaviour
    {
        private const ulong RoomStream = 1;
        private const ulong SpawnStream = 2;
        private const ulong LootStream = 3;
        private const float RoomMargin = 2.5f;
        private const float PortalFromExit = 3f;

        private static readonly Vector3 ArrivalOffset = new Vector3(3f, 0.02f, 0f);

        [SerializeField] private Room hubRoom;
        [SerializeField] private Room restRoom;
        [SerializeField] private Room exitRoom;
        [SerializeField] private Room[] roomPool;
        [SerializeField] private EnemyController[] enemyPool;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private Progression progression;
        [SerializeField] private Portal portalPrefab;
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private int mapsPerRun = 2;
        [SerializeField] private int roomsPerMap = 2;
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

        /// <summary>The arena at the end of the run.</summary>
        public BossArena Arena { get; private set; }

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

            Room previous = Instantiate(hubRoom, transform);
            previous.PlaceEntryAt(Vector3.zero);
            BindRoom(previous);
            player.transform.position = previous.PlayerStart.position;
            Portal pending = previous.GetComponentInChildren<Portal>();

            var difficulty = new DifficultyCurve(statStepPerRoom, baseSpawnChance, spawnChanceStep);
            int[] chain = RoomChain.Build(roomRandom, roomPool.Length, mapsPerRun * roomsPerMap);
            for (int depth = 0; depth < chain.Length; depth++)
            {
                previous = PlaceAfter(roomPool[chain[depth]], previous);
                SpawnEnemies(previous, spawnRandom, difficulty, depth);
                SpawnLoot(previous, lootRandom);

                if (depth % roomsPerMap == 0)
                {
                    pending.SetDestination(previous.Entry.position + ArrivalOffset);
                }

                if ((depth + 1) % roomsPerMap == 0)
                {
                    Portal mapExit = PlacePortal(previous);
                    previous = PlaceAfter(restRoom, previous);
                    mapExit.SetDestination(previous.PlayerStart.position);
                    pending = previous.GetComponentInChildren<Portal>();
                }
            }

            Room last = PlaceAfter(exitRoom, previous);
            pending.SetDestination(last.Entry.position + ArrivalOffset);
            last.GetComponentInChildren<ExitGate>(true).Reached += OnExitReached;
            Arena = last.GetComponent<BossArena>();
            Arena.Prepare(player.transform);
            Arena.Boss.Defeated += OnBossDefeated;

            // Rooms were moved after they were created. Without this, the first physics step
            // still sees their colliders at the origin and everything starts falling.
            UnityEngine.Physics2D.SyncTransforms();
        }

        private Room PlaceAfter(Room prefab, Room previous)
        {
            Room room = Instantiate(prefab, transform);
            room.PlaceEntryAt(previous.Exit.position);
            BindRoom(room);
            return room;
        }

        private Portal PlacePortal(Room room)
        {
            Vector3 position = room.Exit.position + Vector3.left * PortalFromExit;
            Portal portal = Instantiate(portalPrefab, position, Quaternion.identity, room.transform);
            portal.Used += OnPortalUsed;
            return portal;
        }

        private void BindRoom(Room room)
        {
            foreach (Altar altar in room.GetComponentsInChildren<Altar>())
            {
                altar.Bind(progression);
            }

            foreach (Portal portal in room.GetComponentsInChildren<Portal>())
            {
                portal.Used += OnPortalUsed;
            }
        }

        // Without this the camera would slide across the level after a portal.
        private void OnPortalUsed(Vector2 moved)
        {
            followCamera.OnTargetObjectWarped(player.transform, moved);
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
                    enemy.Configure(player.transform, room.Entry.position.x + RoomMargin, room.Exit.position.x - RoomMargin, difficulty.StatScale(depth), random.Fork((ulong)i + 1));
                    enemy.Killed += progression.DropEmbers;
                }
            }
        }

        private void SpawnLoot(Room room, SeededRandom random)
        {
            var weights = new int[loot.Length];
            for (int i = 0; i < loot.Length; i++)
            {
                weights[i] = progression.IsUnlocked(loot[i].RequiredUnlock) ? loot[i].Weight : 0;
            }

            GameObject prefab = loot[LootTable.Pick(random, weights)].Prefab;
            Instantiate(prefab, room.LootSpawn.position, Quaternion.identity, room.transform);
        }

        private void OnBossDefeated()
        {
            progression.DropEmbers(Arena.Boss.transform.position, Arena.Boss.Embers);
        }

        private void OnPlayerDied()
        {
            EndRun("You died\n" + progression.SettleDeath() + " Embers kept");
        }

        private void OnExitReached()
        {
            EndRun("Run complete\n" + progression.SettleVictory() + " Embers kept");
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
