using System;
using UnityEngine;

public class Ascendance : MonoBehaviour, IResettable
{
    [Header("References")]
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private GameObject particleSpawner, postMortemEye;

    [Header("Ascendance Settings")]
    [field: SerializeField] public float TimeToAscend { get; private set; } = 0.75f;
    [field: SerializeField, Range(0f, 1f)]
    public float VisionOnAscension { get; private set; } = 0.25f;

    public event Action TeamMateAscended;
    public event Action<CharacterMediator> SpiritLeft, OnAscendance;

    public bool HasAscended { get; private set; } = false;
    public bool HasSpiritLeftAlready { get; private set; } = false;
    
    
    private void Start()
    {
        mediator.Died += OnDeath;
    }

    public void Ascend()
    {
        if (mediator.IsLocalPlayer)
        {
            mediator.VisionRange.ModifiableValue.AddOrChangeMultiplier(this, VisionOnAscension);
        }
        HasAscended = true;
        //owner.SpriteRenderer.MultiplyColor(0.1f, 1f, 1f);
        particleSpawner.SetActive(true);
        postMortemEye.SetActive(true);

        OnAscendance?.Invoke(mediator);
    }

    private async void OnDeath(CharacterMediator owner)
    {
        var player = owner.playerData;

        if (GameStateManager.Instance.RoundDecided) return;
        
        if (player is not null)
        {
            await TaskExtensions.Delay(TimeToAscend);
            var teamMate = player.GetTeamMate();
            teamMate.Mediator.Ascendance.Ascend();
            TeamMateAscended?.Invoke();
            SpiritLeft?.Invoke(owner);
            HasSpiritLeftAlready = true;
        }
    }

    public void Reset()
    {
        HasAscended = false;
        HasSpiritLeftAlready = false;
        particleSpawner.SetActive(false);
        postMortemEye.SetActive(false);
    }
}
