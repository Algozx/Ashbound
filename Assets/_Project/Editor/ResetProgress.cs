using Ashbound.Gameplay.Meta;
using UnityEditor;
using UnityEngine;

namespace Ashbound.Editor
{
    /// <summary>Clears the saved profile, to test the game from a new start.</summary>
    public static class ResetProgress
    {
        [MenuItem("Ashbound/Reset Progress")]
        private static void Reset()
        {
            PlayerPrefs.DeleteKey(ProfileStore.Key);
            PlayerPrefs.Save();
        }
    }
}
