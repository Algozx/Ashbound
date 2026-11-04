using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Hit stop, camera shake and sound for the player's fights.</summary>
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource impulse;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] swingSounds;
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip[] hurtSounds;

        private Coroutine hitStop;

        public void PlaySwing()
        {
            Play(swingSounds);
        }

        public void PlayHit(float stopTime, float shakeForce)
        {
            Play(hitSounds);
            impulse.GenerateImpulseWithForce(shakeForce);

            if (hitStop != null)
            {
                StopCoroutine(hitStop);
            }

            hitStop = StartCoroutine(HitStop(stopTime));
        }

        public void PlayHurt(float stopTime, float shakeForce)
        {
            Play(hurtSounds);
            PlayHit(stopTime, shakeForce);
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }

        private void Play(AudioClip[] clips)
        {
            if (clips.Length > 0)
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }

        private IEnumerator HitStop(float seconds)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(seconds);
            Time.timeScale = 1f;
            hitStop = null;
        }
    }
}
