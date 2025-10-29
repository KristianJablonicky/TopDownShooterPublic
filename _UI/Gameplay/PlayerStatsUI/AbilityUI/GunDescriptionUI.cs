using UnityEngine;

public class GunDescriptionUI : MonoBehaviour
{
    public string Text { get; private set; }
    public void Init(CharacterToolkit toolkit)
    {
        Text = toolkit.GetGunDescription();
    }
}
