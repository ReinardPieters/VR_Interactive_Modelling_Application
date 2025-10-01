using UnityEngine;
using TMPro;

public class ToolSwitcher : MonoBehaviour
{
    public TMP_Dropdown toolDropdown;

    public MonoBehaviour straightLineTool;
    public MonoBehaviour freeDrawTool;
    public MonoBehaviour rectangleTool;
    public MonoBehaviour circleTool;
    public MonoBehaviour polygonTool;

    void Start()
    {
        if (toolDropdown != null)
            toolDropdown.onValueChanged.AddListener(SwitchTool);
        SwitchTool(toolDropdown != null ? toolDropdown.value : 0);
    }

    public void SwitchTool(int idx)
    {
        Enable(straightLineTool, idx == 1);
        Enable(freeDrawTool, idx == 2);
        Enable(rectangleTool, idx == 3);
        Enable(circleTool, idx == 4);
        Enable(polygonTool, idx == 5);
    }

    void Enable(MonoBehaviour c, bool on)
    {
        if (c != null) c.enabled = on;
    }
}
