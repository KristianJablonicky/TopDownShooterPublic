using UnityEngine;

public class XButton : MonoBehaviour
{
    public void Clicked() => WindowStackManager.Instance.CloseWindow();
}
