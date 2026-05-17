using UnityEngine;

public class OneTimeAnimation : AnimationBase
{
    [SerializeField] private bool playOnStart = true;

    private void OnEnable()
    {
        if (playOnStart) PlayAnimation();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }
}
