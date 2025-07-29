using UnityEngine;
using UnityEngine.UI;

public class ToolPanelUI : MonoBehaviour
{
    public Button toolsButton;
    public GameObject toolsPanel;

    void Start()
    {
        toolsPanel.SetActive(false); // Hide on start
        toolsButton.onClick.AddListener(ToggleToolsPanel);
    }

    void ToggleToolsPanel()
    {
        toolsPanel.SetActive(!toolsPanel.activeSelf); // Toggle visibility
    }
}
