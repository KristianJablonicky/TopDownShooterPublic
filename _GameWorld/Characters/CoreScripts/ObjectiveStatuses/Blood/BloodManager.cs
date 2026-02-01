using System;
using UnityEngine;

public class BloodManager : MonoBehaviour, IResettable
{
    [SerializeField] private BloodPuddle bloodPuddle;
    private GameObject particleSpawner;
    private BloodPuddle bloodPuddleInstance;
    public bool BloodPickedUp { get; private set; }
    public event Action OnBloodPickedUp;

    private CharacterMediator owner;
    public void Init(CharacterMediator mediator, GameObject particleSpawner)
    {
        owner = mediator;
        this.particleSpawner = particleSpawner;
        mediator.NewRoleAssigned += (newRole) => OnNewRoleAssigned(mediator, newRole);
        mediator.Died += OnDeath;
    }
    private void OnNewRoleAssigned(CharacterMediator mediator, Role newRole)
    {
        if (newRole == Role.Attacker)
        {
            BloodPickedUp = false;
        }
    }

    private void OnDeath(CharacterMediator mediator)
    {
        if (mediator.Role == Role.Attacker
            || BloodPickedUp)
        {
            bloodPuddleInstance = Instantiate(
                bloodPuddle,
                mediator.GetPosition(),
                QuaternionUtilities.Random()
            );
            bloodPuddleInstance.Init(owner.PlayerId);
        }
    }

    public void RequestBloodPickUp(ulong bloodOwnerId)
    {
        owner.NetworkInput.RequestBloodPickUp(bloodOwnerId);
    }

    public void PickUpBlood()
    {
        BloodPickedUp = true;
        //owner.SpriteRenderer.MultiplyColor(1f, 0.5f, 0.5f);
        particleSpawner.SetActive(true);
        OnBloodPickedUp?.Invoke();
    }

    public void CleanUpBlood()
    {
        if (bloodPuddleInstance == null)
        {
            Debug.LogWarning("bloodPuddleInstance == null");
            return;
        }

        bloodPuddleInstance.CleanUp();
    }

    public void Reset()
    {
        BloodPickedUp = false;
        particleSpawner.SetActive(false);
    }
}
