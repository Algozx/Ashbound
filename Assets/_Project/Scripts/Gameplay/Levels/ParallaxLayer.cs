using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>A repeating picture that follows the camera and scrolls slower than the world.</summary>
    [DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("Scrolling speed compared to the world. Zero never moves, one moves with the level.")]
        [SerializeField, Range(0f, 1f)] private float scroll = 0.3f;
        [SerializeField] private Transform followed;

        private Vector2 tile;

        private void Awake()
        {
            tile = GetComponent<SpriteRenderer>().sprite.bounds.size;
        }

        // Runs after the camera moved, or the layer would shake against it.
        private void LateUpdate()
        {
            Vector3 camera = followed.position;
            float x = camera.x - Mathf.Repeat(camera.x * scroll, tile.x);
            float y = camera.y - Mathf.Repeat(camera.y * scroll, tile.y);
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
