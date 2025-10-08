using UnityEngine;

public abstract class Ability : ScriptableObject, IResettable
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField, TextArea] public string LongDescription { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    protected CharacterMediator owner;
    protected AbilityRPCs characterRPCs;
    public Ability Factory(CharacterMediator owner)
    {
        var instance = Instantiate(this);
        instance.owner = owner;
        instance.SetUp();
        return instance;
    }
    public void SetAbilityRPC(AbilityRPCs rpcs)
    {
        characterRPCs = rpcs;
        SetUpRPCsReady();
    }

    protected abstract void SetUp();
    protected virtual void SetUpRPCsReady() { }

    public void Reset()
    {
        AbstractReset();
    }
    protected abstract void AbstractReset();
}