using UnityEngine;

public class FollowGO : MonoBehaviour
{
    [SerializeField] private GameObject followedGO;

    private void LateUpdate()
    {
        if (followedGO != null)
        {
            transform.position = followedGO.transform.position;
        }
        else
        {
            enabled = false;
        }
    }
}
