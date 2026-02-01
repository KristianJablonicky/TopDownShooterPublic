using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PracticeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField, Range(0f, 1f)] private float floorSwitchChance = 0.5f;
    [SerializeField, Range(0f, 0.5f),
        Tooltip("Percentage of spawn positions occupied by default")]
        private float dummyDensity = 0.25f;

    [Header("References")]
    [SerializeField] private CharacterMediator[] firstFloorDummies, secondFloorDummies;

    private TrainingScoreManager scoreManager;

    public void Init(TrainingScoreManager scoreManager)
    {
        SubscribeToCharacters(Floor.Basement, firstFloorDummies);
        SubscribeToCharacters(Floor.Outside, secondFloorDummies);
        this.scoreManager = scoreManager;

    }

    public void StartTraining()
    {
        InitDummies(firstFloorDummies);
        InitDummies(secondFloorDummies);
    }
    private void InitDummies(CharacterMediator[] dummies)
    {
        var countToActivate = Mathf.RoundToInt(dummies.Length * dummyDensity);
        var shuffled = dummies.OrderBy(_ => Random.value).ToArray();
        for (int i = 0; i < countToActivate; i++)
        {
            shuffled[i].gameObject.SetActive(true);
            shuffled[i].Reset();
        }
    }

    private void SubscribeToCharacters(Floor floor, CharacterMediator[] dummies)
    {
        foreach (var dummy in dummies)
        {
            dummy.Died += ch =>
            {
                StartCoroutine(OnDummyDeath(ch, floor));
            };
            dummy.SetActivity(false);
        }
    }

    public void DisableDummies()
    {
        DisableDummies(firstFloorDummies);
        DisableDummies(secondFloorDummies);
        StopAllCoroutines();
    }

    private void DisableDummies(CharacterMediator[] dummies)
    {
        foreach (var dummy in dummies)
        {
            dummy.SetActivity(false);
            dummy.HealthComponent.CurrentHealth.Set(0);
            //dummy.HealthComponent.TakeLethalDamage();
        }
    }


    private IEnumerator OnDummyDeath(CharacterMediator character, Floor floor)
    {
        scoreManager.AddScoreForTakeDown();

        yield return new WaitForSeconds(respawnDelay);
        
        if (Random.value <= floorSwitchChance)
        {
            floor = FloorUtilities.GetDifferentFloor(floor);
        }
        RespawnDummy(floor == Floor.Basement ? firstFloorDummies : secondFloorDummies);
    }

    private void RespawnDummy(CharacterMediator[] dummies)
    {
        var inactiveDummies = dummies.Where(d => !d.IsAlive).ToList();

        if (inactiveDummies.Count == 0)
        {
            Debug.LogError("No inactive dummies available for respawn!");
            return;
        }

        var selected = inactiveDummies[Random.Range(0, inactiveDummies.Count)];
        
        selected.gameObject.SetActive(true);
        selected.Reset();
    }
}
