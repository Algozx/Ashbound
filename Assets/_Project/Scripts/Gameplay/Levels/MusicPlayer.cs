using System.Collections;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Plays the music of each part of a run, fading between tracks.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private RunDirector run;
        [SerializeField] private AudioClip calmTrack;
        [SerializeField] private AudioClip mapTrack;
        [SerializeField] private AudioClip bossTrack;
        [SerializeField] private float bossPitch = 1.15f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
        [SerializeField] private float fadeTime = 0.8f;

        private AudioSource source;
        private Coroutine fade;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
        }

        private void OnEnable()
        {
            run.MapChanged += OnMapChanged;
        }

        private void OnDisable()
        {
            run.MapChanged -= OnMapChanged;
        }

        private void Start()
        {
            run.Arena.Started += OnBossFightStarted;
            run.Arena.Boss.Defeated += OnBossDefeated;
            source.clip = calmTrack;
            source.volume = volume;
            source.Play();
        }

        private void OnMapChanged(GameMap map)
        {
            Play(map != null ? mapTrack : calmTrack, 1f);
        }

        private void OnBossFightStarted()
        {
            Play(bossTrack, bossPitch);
        }

        private void OnBossDefeated()
        {
            Play(calmTrack, 1f);
        }

        private void Play(AudioClip clip, float pitch)
        {
            if (fade != null)
            {
                StopCoroutine(fade);
            }

            fade = StartCoroutine(FadeTo(clip, pitch));
        }

        private IEnumerator FadeTo(AudioClip clip, float pitch)
        {
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(volume, 0f, t / fadeTime);
                yield return null;
            }

            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.Play();
        }
    }
}
