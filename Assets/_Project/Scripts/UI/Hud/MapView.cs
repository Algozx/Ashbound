using Ashbound.Gameplay.Levels;
using Ashbound.Logic.Levels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Draws the parts of the current map the player has seen, and toggles with a button.</summary>
    public sealed class MapView : MonoBehaviour
    {
        private const int RevealRadius = 11;
        private const int MarkRadius = 1;

        private static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

        [SerializeField] private RunDirector run;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject window;
        [SerializeField] private RawImage image;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private Vector2 maxSize = new Vector2(1120f, 560f);
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private Color floorColor = new Color(0.12f, 0.16f, 0.3f, 0.95f);
        [SerializeField] private Color wallColor = new Color(0.6f, 0.65f, 0.75f, 1f);
        [SerializeField] private Color safeExitColor = new Color(0.45f, 0.85f, 0.6f);
        [SerializeField] private Color dangerExitColor = new Color(0.95f, 0.3f, 0.25f);
        [SerializeField] private Color treasureColor = new Color(1f, 0.75f, 0.3f);

        private GameMap map;
        private MapFog fog;
        private Texture2D texture;
        private BoundsInt cells;

        private void Awake()
        {
            window.SetActive(false);
        }

        private void OnEnable()
        {
            toggleAction.action.Enable();
            run.MapChanged += Show;
        }

        private void OnDisable()
        {
            run.MapChanged -= Show;
        }

        private void OnDestroy()
        {
            Destroy(texture);
        }

        private void Show(GameMap newMap)
        {
            map = newMap;
            window.SetActive(false);
            if (map == null)
            {
                return;
            }

            cells = map.Solids.cellBounds;
            fog = new MapFog(cells.size.x, cells.size.y);
            Destroy(texture);
            texture = new Texture2D(cells.size.x, cells.size.y, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixels(new Color[cells.size.x * cells.size.y]);
            texture.Apply();
            image.texture = texture;

            float scale = Mathf.Min(maxSize.x / cells.size.x, maxSize.y / cells.size.y);
            image.rectTransform.sizeDelta = new Vector2(cells.size.x, cells.size.y) * scale;
        }

        private void Update()
        {
            if (map == null)
            {
                return;
            }

            if (toggleAction.action.WasPressedThisFrame())
            {
                window.SetActive(!window.activeSelf);
            }

            Vector3Int cell = map.Solids.WorldToCell(player.position) - cells.min;
            if (fog.Reveal(cell.x, cell.y, RevealRadius))
            {
                Paint(cell.x, cell.y);
            }

            Vector2 size = image.rectTransform.sizeDelta;
            playerMarker.anchoredPosition = new Vector2((cell.x + 0.5f) / fog.Width * size.x, (cell.y + 0.5f) / fog.Height * size.y);
        }

        private void Paint(int centerX, int centerY)
        {
            for (int y = centerY - RevealRadius; y <= centerY + RevealRadius; y++)
            {
                for (int x = centerX - RevealRadius; x <= centerX + RevealRadius; x++)
                {
                    if (fog.IsRevealed(x, y))
                    {
                        texture.SetPixel(x, y, ColorOf(x, y));
                    }
                }
            }

            Mark(map.SafeExit.transform, safeExitColor);
            Mark(map.DangerExit.transform, dangerExitColor);
            foreach (Transform spot in map.TreasureSpots)
            {
                Mark(spot, treasureColor);
            }

            texture.Apply();
        }

        private Color ColorOf(int x, int y)
        {
            if (!IsSolid(x, y))
            {
                return floorColor;
            }

            bool edge = !IsSolid(x + 1, y) || !IsSolid(x - 1, y) || !IsSolid(x, y + 1) || !IsSolid(x, y - 1);
            return edge ? wallColor : Clear;
        }

        private bool IsSolid(int x, int y)
        {
            return map.Solids.HasTile(new Vector3Int(x + cells.xMin, y + cells.yMin, 0));
        }

        private void Mark(Transform target, Color color)
        {
            Vector3Int cell = map.Solids.WorldToCell(target.position) - cells.min;
            if (!fog.IsRevealed(cell.x, cell.y))
            {
                return;
            }

            for (int y = -MarkRadius; y <= MarkRadius; y++)
            {
                for (int x = -MarkRadius; x <= MarkRadius; x++)
                {
                    texture.SetPixel(cell.x + x, cell.y + y + MarkRadius, color);
                }
            }
        }
    }
}
