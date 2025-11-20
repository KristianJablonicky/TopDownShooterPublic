using UnityEngine;

public class WarmUpManager : MonoBehaviour
{
    private void Start()
    {
        GameStateManager.Instance.GameStarted += () => Destroy(gameObject);
    }
}
