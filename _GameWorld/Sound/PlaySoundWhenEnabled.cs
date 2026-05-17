using System.Collections;
using UnityEngine;

public class PlaySoundWhenEnabled : MonoBehaviour
{
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private bool randomizePitch = true;
    private void OnEnable()
    {
        soundPlayer.RequestPlaySound(transform, clips, randomizePitch);
        //StartCoroutine(Request());
    }

    private IEnumerator Request()
    {
        yield return null; // wait a frame to ensure everything is set up
        Debug.Log($"Requesting sound play for {gameObject.name} at position {transform.position}");
        soundPlayer.RequestPlaySound(transform, clips, randomizePitch);
    }
}
