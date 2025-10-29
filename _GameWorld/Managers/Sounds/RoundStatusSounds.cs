using UnityEngine;

public class RoundStatusSounds : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip roundWonSound, roundLostSound,
        gameWonSound, gameLostSound,
        roundStart;
    private void Start()
    {
        var stateManager = GameStateManager.Instance;
        stateManager.RoundWon += (won) => PlaySound(won, roundWonSound, roundLostSound);
        stateManager.GameWon += (won) => PlaySound(won, gameWonSound, gameLostSound);
        stateManager.NewRoundStarted += () => source.PlayOneShot(roundStart);

        stateManager.TrainingStarted += () => source.PlayOneShot(roundStart);
        stateManager.TrainingEnded += newHighScore =>
        source.PlayOneShot(newHighScore ? gameWonSound : roundLostSound);
    }

    private void PlaySound(bool won, AudioClip winSound, AudioClip lossSound)
    {
        source.PlayOneShot(won ? winSound : lossSound);
    }
}
