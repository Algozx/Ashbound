using Ashbound.Logic.Meta;
using UnityEngine;

namespace Ashbound.Gameplay.Meta
{
    /// <summary>Keeps the profile as JSON in the browser's storage.</summary>
    public static class ProfileStore
    {
        public const string Key = "ashbound.profile";

        public static Profile Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return new Profile();
            }

            try
            {
                return Profile.FromData(JsonUtility.FromJson<ProfileData>(json));
            }
            catch (System.ArgumentException)
            {
                Debug.LogWarning("The saved profile could not be read, so a new one was started.");
                return new Profile();
            }
        }

        public static void Save(Profile profile)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(profile.ToData()));
            PlayerPrefs.Save();
        }
    }
}
