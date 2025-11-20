using System;
using UnityEngine;

public class Updater : SingletonMonoBehaviour<Updater>
{
    public event Action<float> Updated;

    private void Update()
    {
        Updated?.Invoke(Time.deltaTime);
    }
}
