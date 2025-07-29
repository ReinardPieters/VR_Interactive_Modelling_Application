using UnityEngine;
using TMPro;

public class ToolDropdownManager : MonoBehaviour
{
    public TMP_Dropdown toolDropdown;

    void Start()
    {
        toolDropdown.ClearOptions();  // Clear any default values

        // Create a list of tool options
        var toolOptions = new System.Collections.Generic.List<string>
        {
            "Select Tool",
            "Straight Line",
            "Free Draw",
            "Rectangle",
            "Eraser"
        };

        toolDropdown.AddOptions(toolOptions);
    }
}
