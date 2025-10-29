using UnityEngine;

public class ModifierManagerUI : MonoBehaviour
{
    [SerializeField] private ModifierUpdater modifierUpdaterPrefab;
    [SerializeField] private Transform modifierListGameObject;

    private void Start()
    {
        PlayerNetworkInput.OwnerSpawned += OnOwnerSpawn;
    }

    private void OnOwnerSpawn(CharacterMediator mediator)
    {
        mediator.Modifiers.ModifierAdded += OnModifierAdded;
    }

    private void OnModifierAdded(Modifier modifier)
    {
        var newModifierUI = Instantiate(modifierUpdaterPrefab, modifierListGameObject);
        newModifierUI.UpdateModifierUI(modifier);
    }
}
