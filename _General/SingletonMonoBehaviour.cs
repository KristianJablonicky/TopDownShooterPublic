using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this as T)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        OverriddenAwake();
    }

    /// <summary>
    /// Override this instead of Awake in subclasses.
    /// </summary>
    protected virtual void OverriddenAwake() { }
}