using UnityEngine;

public class DestroyOnRoundEnd : MonoBehaviour
{
    private void Start()
    {
        VirtualStart();

        var manager = GameStateManager.Instance;
        if (!manager.GameInProgress) return;

        manager.RoundEnded += CleanUp;
        manager.NewRoundStarted += CleanUp; // if object got created after round end.
    }
    private void CleanUp()
    {
        VirtualCleanUp();
        Destroy(gameObject);
    }
    protected virtual void VirtualStart() { }
    protected virtual void VirtualCleanUp() { }

    private void OnDestroy()
    {
        var manager = GameStateManager.Instance;
        manager.RoundEnded -= CleanUp;
        manager.NewRoundStarted -= CleanUp;
    }
}
