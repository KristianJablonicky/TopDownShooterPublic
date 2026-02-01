using UnityEngine;

public class HitMarkers : MonoBehaviour
{
    [SerializeField] private HitMarker[] hitMarkers;

    [SerializeField] private float headShotAnimationDuration = 0.25f,
        maxAngle = 25f,
        maxAngleRandomness = 0.2f;

    private void Start()
    {
        PlayerNetworkInput.PlayerSpawned += mediator => mediator.NetworkInput.OnDamageDealt += OnDamageDealt;
    }
    private void OnDamageDealt(int damage, DamageTag tag)
    {
        var headshot = tag == DamageTag.HeadShot;
        foreach (var hitMarker in hitMarkers)
        {
            hitMarker.DamageDealt(damage, headshot);
        }
        if (!headshot) return;

        StopAllCoroutines();

        var maxAngleRoll = maxAngle * (1f - Random.Range(0f, maxAngleRandomness));
        maxAngleRoll *= Random.value < 0.5f ? -1f : 1f;

        StartCoroutine(Tweener.TweenCoroutine(
            this, 0f, maxAngleRoll,
            headShotAnimationDuration, TweenStyle.sinusPingPong,
            value => transform.localEulerAngles = new(0f, 0f, value)
            )
        );
    }
}
