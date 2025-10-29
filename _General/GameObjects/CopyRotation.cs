using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    private GameObject copiedGO;
    private void Start()
    {
        enabled = false;
    }
    
    public void Copy(GameObject go)
    {
        copiedGO = go;
        enabled = true;
    }
    
    private void LateUpdate()
    {
        transform.rotation = copiedGO.transform.rotation;
    }
}
