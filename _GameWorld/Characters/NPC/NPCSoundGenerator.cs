using System.Collections;
using UnityEngine;

public class NPCSoundGenerator : MonoBehaviour
{
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float interval = 5f;
    [SerializeField] private float maxInitialDelay = 2f;
    [SerializeField, Range(0f, 1f)] private float randomness;

    private Coroutine playingCoroutine;
    private float initialDelay = 0f;

    private bool ignoreInitialEnable = true;

    private void OnDisable()
    {
        if (playingCoroutine != null)
        {
            initialDelay = maxInitialDelay;
            StopCoroutine(playingCoroutine);
        }
    }

    private void OnEnable()
    {
        if (ignoreInitialEnable)
        {
            ignoreInitialEnable = false;
            return;
        }
        playingCoroutine = StartCoroutine(PlaySound());
    }

    private IEnumerator PlaySound()
    {
        var clip = audioClips[Random.Range(0, audioClips.Length)];
        while (true)
        {
            if (initialDelay > 0f)
            {
                initialDelay = 0f;
                yield return new WaitForSeconds
                (
                    RandomizeDelay(initialDelay)
                );
            }
            else
            {
                yield return new WaitForSeconds
                (
                    RandomizeDelay(interval)
                );
            }
            soundPlayer.RequestPlaySound(transform, clip, true);
        }
    }

    private float RandomizeDelay(float delay) => (1f + Random.Range(-randomness, randomness)) * delay;
}
