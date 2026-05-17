using UnityEngine;
using UnityEngine.UI;

public class SubmenuSelectionHandler : MonoBehaviour
{
    [SerializeField] private SettingsSubMenuButton[] subMenuButtons;
    [SerializeField] private ScrollRect scrollRect;

    private SettingsSubMenuButton currentSubmenu;
    
    private void Start()
    {
        foreach (var button in subMenuButtons)
        {
            button.Clicked += OnButtonClick;
            DisableSubmenu(button);
        }

        EnableSubMenu(subMenuButtons[0]);
    }

    private void OnButtonClick(SettingsSubMenuButton button)
    {
        if (button == currentSubmenu) return;
        DisableSubmenu(currentSubmenu);
        EnableSubMenu(button);
    }

    private void DisableSubmenu(SettingsSubMenuButton subMenu)
    {
        subMenu.GetDeactivated();
        subMenu.RectTransform.gameObject.SetActive(false);
    }
    private void EnableSubMenu(SettingsSubMenuButton subMenu)
    {
        //subMenu.GetActivated(); // Stack overflow
        subMenu.RectTransform.gameObject.SetActive(true);
        scrollRect.content = subMenu.RectTransform;
        currentSubmenu = subMenu;
    }
}
