using System;
using UnityEngine;

public abstract class Ability : ScriptableObject, IResettable
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [SerializeField, TextArea] private string longDescription;
    [field: SerializeField] public Sprite Icon { get; private set; }
    [SerializeField] private AudioClip[] audioClips;

    public event Action<Sprite> IconChanged;

    protected CharacterMediator owner;
    protected AbilityRPCs characterRPCs;
    private SoundPlayer soundPlayer;

    public Ability Factory(CharacterMediator owner)
    {
        var instance = Instantiate(this);
        instance.owner = owner;
        instance.SetUp();
        return instance;
    }


    public string GetDescription() => $"{Description}\n{_GetAbilitySuffix()}";
    public string GetLongDescription()
    {
        return $"{longDescription}\n\n{_GetAbilitySpecificStats()}\n{_GetAbilitySuffix()}";
    }
    protected abstract string _GetAbilitySpecificStats();
    protected abstract string _GetAbilitySuffix();

    public void SetAbilityRPC(AbilityRPCs rpcs)
    {
        characterRPCs = rpcs;
        SetUpRPCsReady();
    }

    protected bool TryInvokeRPC<T>(Action<T> rpcAction) where T : AbilityRPCs
    {
        if (characterRPCs is T typedRPCs)
        {
            rpcAction(typedRPCs);
            return true;
        }

        Debug.LogError($"{Name} Expected RPC type {typeof(T).Name}, but got {characterRPCs?.GetType().Name ?? "null"}.");
        return false;
    }

    protected abstract void SetUp();
    protected virtual void SetUpRPCsReady() { }

    public void Reset()
    {
        AbstractReset();
    }
    protected abstract void AbstractReset();

    protected void ChangeIcon(Sprite sprite)
    {
        IconChanged?.Invoke(sprite);
    }
    public void SetSoundPlayer(SoundPlayer soundPlayer) => this.soundPlayer = soundPlayer;

    protected void PlaySound(int? clipIndex = null)
    {
        if (clipIndex.HasValue)
        {
            characterRPCs.RequestPlaySoundRpc(GetAbilityType(), clipIndex.Value);
        }
        else
        {
            characterRPCs.RequestPlaySoundRpc(GetAbilityType());
        }
    }
    public void RpcInvokedPlaySound(int? clipIndex = null)
    {
        PlaySound(GetClip(clipIndex));
    }

    private AudioClip GetClip(int? clipIndex)
    {
        if (clipIndex.HasValue)
        {
            return audioClips[clipIndex.Value];
        }

        return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
    }


    private void PlaySound(AudioClip clip)
    {
        soundPlayer.RequestPlaySound(owner.MovementController.transform, clip, false);
    }

    
    public abstract AbilityType GetAbilityType();

}

public abstract class PassiveAbility : Ability
{
    protected override string _GetAbilitySuffix() => "";
    public override AbilityType GetAbilityType() => AbilityType.Passive;
}