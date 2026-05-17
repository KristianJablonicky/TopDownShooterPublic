using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    [SerializeField] private GameObject copiedGO;
    [SerializeField] private bool simplyFreezeRotation = false;
    private void Start()
    {
        if (copiedGO == null && !simplyFreezeRotation)
        {
            enabled = false;
        }
    }
    
    public void Copy(GameObject go)
    {
        copiedGO = go;
        enabled = true;
    }
    
    private void LateUpdate()
    {
        if (copiedGO != null)
        {
            transform.rotation = copiedGO.transform.rotation;
        }
        else if (simplyFreezeRotation)
        {
            transform.rotation = Quaternion.identity;
        }
        else
        {
            enabled = false;
        }
    }
}
