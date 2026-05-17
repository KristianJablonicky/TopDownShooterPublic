using TMPro;
using UnityEngine;

public class GunCardIcon : MonoBehaviour
{
    [SerializeField] private TMP_Text value;

    public void SetValue(float value)
    {
        this.value.text = value.ToString();
    }
}
