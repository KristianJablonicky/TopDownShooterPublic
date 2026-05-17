using UnityEngine;

public class AbilityButtonMenu : MonoBehaviour
{
    [SerializeField] private CardSetUp card;
    public void OnClick()
    {
        TextDumpPopUpManager.Instance.ShowText(card.Name, card.LongDescription, true, card.ScriptableObjectBase.Icon);
    }
}
