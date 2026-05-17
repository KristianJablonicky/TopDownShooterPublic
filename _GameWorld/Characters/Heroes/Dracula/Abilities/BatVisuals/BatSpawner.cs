using UnityEngine;

public class BatSpawner : MonoBehaviour
{
    [SerializeField] private MoveToPosition[] bats;
    public void SetRadius(float radius)
    {
        foreach (var bat in bats)
        {
            bat.transform.localPosition = Vector2.zero;
            bat.gameObject.SetActive(true);
            bat.SetTargetPosition(new(radius, radius), true);
        }
    }
}
