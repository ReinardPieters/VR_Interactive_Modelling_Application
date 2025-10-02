using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridOverlay : MonoBehaviour
{
	public Renderer quadRenderer; // Assign the drawing quad
	public Material gridMaterial; // Material with grid texture
	public float gridSize = 0.32f; // default = 1cm


	public TMP_InputField WidthInputField;
	public TMP_InputField HeightInputField;
	public Button ResizeButton;

	private Material matInstance;

	void Start()
	{
		if (quadRenderer != null && gridMaterial != null)
		{
			matInstance = Instantiate(gridMaterial);
			quadRenderer.material = matInstance;
			UpdateGrid();
		}

		ResizeButton.onClick.AddListener(ReadNumber);
	}

	public void SetGridSize(float size)
	{
		gridSize = size;
		UpdateGrid();
	}

	void UpdateGrid()
	{
		if (matInstance == null || quadRenderer == null) return;

		// 1. Get actual size of the drawing plane in world units
		Vector3 planeSize = quadRenderer.bounds.size;
		float width = planeSize.x;
		float height = planeSize.y;

		// 2. How many grid cells fit across each axis
		float tilesX = width / gridSize;
		float tilesY = height / gridSize;

		// 3. Apply tiling to material
		matInstance.mainTextureScale = new Vector2(tilesX, tilesY);

		Debug.Log($"Grid updated: {tilesX}x{tilesY} cells for {width}x{height} plane");
		Debug.Log($"Plane X: {width}, Plane Y: {height}");
	}

	public void ReadNumber()
	{
		string Wtext = WidthInputField.text;
		string Htext = HeightInputField.text;

		if (int.TryParse(Wtext, out int newWidth) && int.TryParse(Htext, out int newHeight))
		{
			quadRenderer.transform.localScale = new Vector3(newWidth / 50f, newHeight / 50f, 1f);
			Debug.Log($"Plane X: {quadRenderer.bounds.size.x}, Plane Y: {quadRenderer.bounds.size.y}");
			UpdateGrid();
		}
		else
		{
			Debug.LogWarning("Invalid number input: " + Wtext + ", " + Htext);
		}
	}
}
