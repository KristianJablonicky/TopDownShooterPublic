using UnityEditor;
using UnityEngine;

public class DeathDissolveMediator : MonoBehaviour
{
    [SerializeField] private DeathDissolveManager deathDissolveManager;
    [SerializeField] private float duration = 2f, bonusScale;
    [SerializeField, Tooltip("Set alpha to 0 to keep default dissolve color")] private Color dissolveColor;
    [SerializeField, Range(0f, 5f)] private float bonusYLow, bonusYHigh;
    [SerializeField, Range(0f, 1f)] private float bonusXLow, bonusXHigh;

    public void Dissolve(CharacterMediator mediator)
    {
        var dissolveInstance = Instantiate(deathDissolveManager, mediator.GetPosition(), mediator.GetTransform().rotation);
        var bonusXPos = Random.Range(bonusXLow, bonusXHigh);
        if (Random.value < 0.5f) bonusXPos *= -1f;
        var bonusYPos = Random.Range(bonusYLow, bonusYHigh);
        dissolveInstance.Dissolve(mediator, duration,
            mediator.MovementController.GetLinearVelocity(),
            bonusScale, dissolveColor);
    }
}
