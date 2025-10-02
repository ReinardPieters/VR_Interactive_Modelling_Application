using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridMenu : MonoBehaviour
{
	public GridOverlay gridOverlay;
	public TMP_Dropdown gridSizeDropdown;

	public void SetGrid1mm() => gridOverlay.SetGridSize(0.1f);
	public void SetGrid5mm() => gridOverlay.SetGridSize(0.5f);
	public void SetGrid1cm() => gridOverlay.SetGridSize(1f);
	public void SetGrid5cm() => gridOverlay.SetGridSize(5f);
	public void SetGrid10cm() => gridOverlay.SetGridSize(10f);

	void Awake()
	{
		if (gridOverlay == null)
			gridOverlay = GetComponent<GridOverlay>();
	}

	void Start()
	{
		// Listen for changes
		gridSizeDropdown.onValueChanged.AddListener(OnGridSizeChanged);
	}

	void OnDestroy()
	{
		// Always clean up listeners
		gridSizeDropdown.onValueChanged.RemoveListener(OnGridSizeChanged);
	}

	void OnGridSizeChanged(int index)
	{
		string selected = gridSizeDropdown.options[index].text;

		Debug.Log("Selected Grid Size: " + selected);

		switch (selected)
		{
			case "1 mm":
				gridOverlay.SetGridSize(0.032f);
				break;
			case "5 mm":
				gridOverlay.SetGridSize(0.16f);
				break;
			case "1 cm":
				gridOverlay.SetGridSize(0.32f);
				break;
			case "5 cm":
				gridOverlay.SetGridSize(1.6f);
				break;
			case "10 cm":
				gridOverlay.SetGridSize(3.2f);
				break;
		}
	}
}