using UnityEngine;

public class ModifierPrefab : DestroyOnRoundEnd
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private FadeOutThenGetDestroyed fadeOutThenGetDestroyed;
    [SerializeField] private PopIn popIn;
    private CharacterMediator owner;
    private Modifier modifier;
    public void SetUp(CharacterMediator owner, Modifier modifier)
    {
        this.owner = owner;
        this.modifier = modifier;
        owner.Died += CleanUp;
        modifier.Expired += CleanUp;

        transform.SetParent(owner.GetTransform());
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        if (owner.IsLocalPlayer)
        {
            // make always visible if local player
            owner.AnimationController.SetUpSpriteRenderer(sr, 5);
        }
    }

    private void CleanUp(CharacterMediator owner)
    {
        CleanUp();
    }
    private void CleanUp()
    {
        owner.Died -= CleanUp;
        modifier.Expired -= CleanUp;
        if (fadeOutThenGetDestroyed != null)
        {
            fadeOutThenGetDestroyed.PlayAnimation(null);
        }
        else if (popIn != null)
        {
            popIn.PlayAnimation();
            Destroy(gameObject, popIn.GetDuration() + 1f);
        }
        else if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
