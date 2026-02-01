using UnityEngine;

public abstract class SinusUpdater : MonoBehaviour
{
    [SerializeField] private float frequency = 1f;
    [SerializeField] private bool randomizeStart = false;
    private float timeElapsed = 0f;

    private void Start()
    {
        if (randomizeStart)
        {
            timeElapsed = Random.Range(0f, 2f * Mathf.PI / frequency);
        }
    }
    float sinusValue;
    private void Update()
    {
        timeElapsed += Time.deltaTime;
        sinusValue = Mathf.Sin(Mathf.PI * timeElapsed * frequency);
        UpdateSinus(sinusValue, (sinusValue + 1) * 0.5f);
    }
    protected abstract void UpdateSinus(float sinusValue, float sinus01);
}
