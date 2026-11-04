using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashbound.Core
{
    /// <summary>Entry point of the build: starts services, then loads the first playable scene.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(SceneNames.Game);
        }
    }
}
