using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    [SerializeField] private GameObject copiedGO;
    private void Start()
    {
        if (copiedGO == null)
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
        else
        {
            enabled = false;
        }
    }
}
