using TMPro;
using UnityEngine;

public class VersionTextUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text version;
    private void Awake()
    {
        version.text = $"Version: {Application.version}";
    }
}
