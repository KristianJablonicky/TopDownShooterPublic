using UnityEngine;

public class FollowGO : MonoBehaviour
{
    [SerializeField] private GameObject followedGO;

    private void LateUpdate()
    {
        transform.position = followedGO.transform.position;
    }
}
