using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierUpdater : MonoBehaviour
{
    [SerializeField] private Image modifierIcon, modifierDurationImage;
    [SerializeField] private JaggedOutline jaggedOutline;
    [SerializeField] private TMP_Text stacks, description;

    [SerializeField] private Color buffColor = Color.darkBlue,
        debuffColor = Color.darkRed;

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

        jaggedOutline.SetColor(GetColorBasedOnType(modifier.Type));
    }

    private void OnDurationChanged(float newDuration)
    {
        modifierDurationImage.fillAmount = 1f - newDuration / maxDuration;
    }

    private void OnStacksChanged(int newStacks)
    {
        if (newStacks < 2) stacks.text = string.Empty;
        else stacks.text = newStacks.ToString();
    }

    private Color GetColorBasedOnType(ModifierType modifierType)
    {
        return modifierType switch
        {
            ModifierType.Buff => buffColor,
            ModifierType.Debuff => debuffColor,
            _ => Color.white,
        };
    }

}
