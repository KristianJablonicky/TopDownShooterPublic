using UnityEngine;

public class BloodPuddle : DestroyOnRoundEnd
{
    [SerializeField] private FadeOutThenGetDestroyed animator;
    private ulong ownerId;
    public void Init(ulong ownerId) => this.ownerId = ownerId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HealthComponent healthComponent))
        {
            var mediator = healthComponent.Mediator;
            if (mediator.Role != Role.Defender) return;

            mediator.BloodManager.RequestBloodPickUp(ownerId);
        }
    }

    public void CleanUp()
    {
        animator.PlayAnimation(null);
        Destroy(this);
    }
}
