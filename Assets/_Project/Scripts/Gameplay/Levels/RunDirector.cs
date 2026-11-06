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
    /// Builds a run from a seed: the hub, then large maps with a rest room after each one, then the
    /// boss. Portals join the parts. A map gets its enemies and treasure when the player enters it,
    /// because the portal taken on the way decides how hard it is.
    /// </summary>
    public sealed class RunDirector : MonoBehaviour
    {
        private const ulong RoomStream = 1;
        private const ulong SpawnStream = 2;
        private const ulong LootStream = 3;
        private const float MapSpacing = 400f;
        private const float RestHeight = -200f;
        private const float EnemyTerritory = 14f;
        private const int DangerDepth = 1;
        private const float MapShutdownDelay = 1f;

        private static readonly Vector3 ArrivalOffset = new Vector3(3f, 0.02f, 0f);

        [SerializeField] private Room hubRoom;
        [SerializeField] private Room restRoom;
        [SerializeField] private Room exitRoom;
        [SerializeField] private GameMap[] mapPool;
        [SerializeField] private EnemyController[] enemyPool;
        [SerializeField] private PlayerCombat player;
        [SerializeField] private Progression progression;
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private int mapsPerRun = 2;
        [SerializeField] private LootEntry[] loot;
        [Header("Difficulty")]
        [Tooltip("Extra enemy health and damage per map, as a share of the base value.")]
        [SerializeField] private float statStepPerRoom = 0.3f;
        [SerializeField, Range(0f, 1f)] private float baseSpawnChance = 0.6f;
        [SerializeField, Range(0f, 1f)] private float spawnChanceStep = 0.2f;
        [Tooltip("Embers from enemies after a danger portal, as a multiple of the normal amount.")]
        [SerializeField] private float dangerEmberScale = 1.5f;
        [Tooltip("Boss health and Embers after a danger portal, as a multiple of the normal amount.")]
        [SerializeField] private float dangerBossScale = 1.25f;

        [Header("Seed")]
        [Tooltip("Plays a specific run. Zero picks a new seed every run.")]
        [SerializeField] private long fixedSeed;
        [SerializeField] private float restartDelay = 2f;

        private GameMap[] maps;
        private GameMap currentMap;
        private PlayerHeat playerHeat;
        private DifficultyCurve difficulty;
        private SeededRandom spawnRandom;
        private SeededRandom lootRandom;
        private bool dangerAhead;
        private bool ending;

        /// <summary>Seed of the current run.</summary>
        public ulong Seed { get; private set; }

        /// <summary>The arena at the end of the run.</summary>
        public BossArena Arena { get; private set; }

        /// <summary>Raised with a message when the run ends, before the restart.</summary>
        public event Action<string> RunEnded;

        /// <summary>Raised with the map the player entered, or null on leaving one.</summary>
        public event Action<GameMap> MapChanged;

        private void Awake()
        {
            playerHeat = player.GetComponent<PlayerHeat>();

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
            spawnRandom = random.Fork(SpawnStream);
            lootRandom = random.Fork(LootStream);
            difficulty = new DifficultyCurve(statStepPerRoom, baseSpawnChance, spawnChanceStep);

            Room hub = PlaceRoom(hubRoom, Vector3.zero);
            player.transform.position = hub.PlayerStart.position;

            int[] order = RoomChain.Build(roomRandom, mapPool.Length, mapsPerRun);
            maps = new GameMap[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                var position = new Vector3(MapSpacing * (i + 1), 0f, 0f);
                maps[i] = Instantiate(mapPool[order[i]], position, Quaternion.identity, transform);
                maps[i].gameObject.SetActive(false);
            }

            Portal next = hub.GetComponentInChildren<Portal>();
            Room rest = null;
            for (int i = 0; i < maps.Length; i++)
            {
                int index = i;
                Connect(next, maps[i].Entry.position, () => EnterMap(index));

                rest = PlaceRoom(restRoom, new Vector3(MapSpacing * (i + 1), RestHeight, 0f));
                Connect(maps[i].SafeExit, rest.PlayerStart.position, () => LeaveMap(false));
                Connect(maps[i].DangerExit, rest.PlayerStart.position, () => LeaveMap(true));
                next = rest.GetComponentInChildren<Portal>();
            }

            Room last = PlaceRoom(exitRoom, rest.Exit.position);
            Connect(next, last.Entry.position + ArrivalOffset, OnBossRoomEntered);
            last.GetComponentInChildren<ExitGate>(true).Reached += OnExitReached;
            Arena = last.GetComponent<BossArena>();
            Arena.Prepare(player.transform);
            Arena.Boss.Defeated += OnBossDefeated;

            // Rooms were moved after they were created. Without this, the first physics step
            // still sees their colliders at the origin and everything starts falling.
            UnityEngine.Physics2D.SyncTransforms();
        }

        private Room PlaceRoom(Room prefab, Vector3 entry)
        {
            Room room = Instantiate(prefab, transform);
            room.PlaceEntryAt(entry);
            foreach (Altar altar in room.GetComponentsInChildren<Altar>())
            {
                altar.Bind(progression);
            }

            return room;
        }

        private void Connect(Portal portal, Vector3 destination, Action onUsed)
        {
            portal.SetDestination(destination);
            portal.Used += moved =>
            {
                // Without this the camera would slide across the level after a portal.
                followCamera.OnTargetObjectWarped(player.transform, moved);
                onUsed();
            };
        }

        private void EnterMap(int index)
        {
            GameMap map = maps[index];
            currentMap = map;
            map.gameObject.SetActive(true);
            UnityEngine.Physics2D.SyncTransforms();
            int depth = index + (dangerAhead ? DangerDepth : 0);
            SpawnEnemies(map, depth, dangerAhead ? dangerEmberScale : 1f);
            foreach (Transform spot in map.TreasureSpots)
            {
                SpawnLoot(spot.position);
            }

            MapChanged?.Invoke(map);
        }

        private void LeaveMap(bool dangerous)
        {
            dangerAhead = dangerous;
            MapChanged?.Invoke(null);
            StartCoroutine(ShutDown(currentMap));
        }

        // Waits so the portal's sound can finish and its prompt can clear.
        private IEnumerator ShutDown(GameMap map)
        {
            yield return new WaitForSeconds(MapShutdownDelay);
            map.gameObject.SetActive(false);
        }

        private void OnBossRoomEntered()
        {
            if (dangerAhead)
            {
                Arena.Boss.Strengthen(dangerBossScale);
            }
        }

        private void SpawnEnemies(GameMap map, int depth, float emberScale)
        {
            Transform[] spawns = map.EnemySpawns;
            bool[] used = RoomChain.PickSpawns(spawnRandom, spawns.Length, difficulty.SpawnChance(depth));
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i])
                {
                    Vector3 position = spawns[i].position;
                    EnemyController prefab = enemyPool[spawnRandom.NextInt(0, enemyPool.Length)];
                    EnemyController enemy = Instantiate(prefab, position, Quaternion.identity, map.transform);
                    float minX = position.x - EnemyTerritory;
                    float maxX = position.x + EnemyTerritory;
                    enemy.Configure(player.transform, minX, maxX, difficulty.StatScale(depth), emberScale, spawnRandom.Fork((ulong)i + 1));
                    enemy.Killed += DropHeatedEmbers;
                }
            }
        }

        private void SpawnLoot(Vector3 position)
        {
            var weights = new int[loot.Length];
            for (int i = 0; i < loot.Length; i++)
            {
                weights[i] = progression.IsUnlocked(loot[i].RequiredUnlock) ? loot[i].Weight : 0;
            }

            GameObject prefab = loot[LootTable.Pick(lootRandom, weights)].Prefab;
            Instantiate(prefab, position, Quaternion.identity, transform);
        }

        private void OnBossDefeated()
        {
            DropHeatedEmbers(Arena.Boss.transform.position, Arena.Boss.Embers);
        }

        private void DropHeatedEmbers(Vector2 position, int amount)
        {
            progression.DropEmbers(position, Mathf.RoundToInt(amount * playerHeat.EmberScale));
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
