using UnityEngine;
using UnityEngine.UI;

public class DropdownMenuUI : MonoBehaviour
{
    public Button mainMenuButton;
    public GameObject menuPanel;

    void Start()
    {
        menuPanel.SetActive(false); 
        mainMenuButton.onClick.AddListener(ToggleMenu);
    }

    void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
