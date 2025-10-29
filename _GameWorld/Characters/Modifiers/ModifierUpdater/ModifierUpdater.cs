using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierUpdater : MonoBehaviour
{
    [SerializeField] private Image modifierIcon, modifierDurationImage;
    [SerializeField] private TMP_Text stacks, description;
    private float maxDuration;
    public void UpdateModifierUI(Modifier modifier)
    {
        if (modifier.Icon != null)
        {
            modifierIcon.sprite = modifier.Icon;
        }
        modifier.Duration.OnValueSet += OnDurationChanged;
        modifier.Stacks.OnValueSet += OnStacksChanged;

        OnStacksChanged(modifier.Stacks);
        maxDuration = modifier.Duration;

        description.text = modifier.Description;

        modifier.Expired += () => Destroy(gameObject);
    }

    private void OnDurationChanged(float newDuration)
    {
        modifierDurationImage.fillAmount = newDuration / maxDuration;
    }

    private void OnStacksChanged(int newStacks)
    {
        stacks.text = newStacks.ToString();
    }
}
