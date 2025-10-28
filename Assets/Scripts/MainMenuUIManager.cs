using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using SFB; // ? moved here

public class MainMenuUIManager : MonoBehaviour
{
	public Transform DrawingQuad;
	public Button newDrawingButton;
	public Button openButton;
	public Button saveButton;
	public Button exportButton;
	public Button exitButton;
	public CloudStorage cloudStorage;

	static readonly CultureInfo CI = CultureInfo.InvariantCulture;

	private string drawingsFolder;
	private string currentSavePath;

	void Start()
	{
		// Initialize save folder
		drawingsFolder = Path.Combine(Application.persistentDataPath, "Drawings");
		if (!Directory.Exists(drawingsFolder))
			Directory.CreateDirectory(drawingsFolder);

		// Wire up button events
		newDrawingButton.onClick.AddListener(NewDrawing);
		openButton.onClick.AddListener(OpenDrawing);
		saveButton.onClick.AddListener(SaveDrawing);
		exportButton.onClick.AddListener(DFXExport);
		exitButton.onClick.AddListener(QuitApplication);

		Debug.Log("Drawings will be saved in: " + drawingsFolder);
	}

	public void NewDrawing()
	{
		for (int i = DrawingQuad.childCount - 1; i >= 0; i--)
			Destroy(DrawingQuad.GetChild(i).gameObject);

		Debug.Log("?? New Drawing Created");
	}

	public void SaveDrawing()
	{
		if (cloudStorage == null)
		{
			Debug.LogError("? CloudStorage reference missing!");
			return;
		}

		string json = cloudStorage.SerializeDrawing();
		string defaultName = "Drawing_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";

		var path = StandaloneFileBrowser.SaveFilePanel("Save Drawing", "", defaultName, "json");
		if (string.IsNullOrEmpty(path))
		{
			Debug.Log("?? Save canceled");
			return;
		}

		File.WriteAllText(path, json, Encoding.UTF8);
		currentSavePath = path;

		Debug.Log($"?? Drawing saved at: {path}");
	}

	public void OpenDrawing()
	{
		if (cloudStorage == null)
		{
			Debug.LogError("? CloudStorage reference missing!");
			return;
		}

		var paths = StandaloneFileBrowser.OpenFilePanel("Open Drawing", "", "json", false);
		if (paths.Length == 0)
		{
			Debug.Log("?? No file selected.");
			return;
		}

		string json = File.ReadAllText(paths[0], Encoding.UTF8);
		cloudStorage.LoadDrawing(json);
		currentSavePath = paths[0];

		Debug.Log($"?? Drawing loaded from: {paths[0]}");
	}

	public void DFXExport()
	{
		StringBuilder sb = new StringBuilder();

		// Start DXF ENTITIES section
		sb.AppendLine("0");
		sb.AppendLine("SECTION");
		sb.AppendLine("2");
		sb.AppendLine("ENTITIES");

		foreach (Transform child in DrawingQuad)
		{
			if (!child.name.Contains("MajorLine")) continue;

			LineRenderer lr = child.GetComponent<LineRenderer>();
			if (lr == null || lr.positionCount < 2) continue;

			Vector3[] positions = new Vector3[lr.positionCount];
			lr.GetPositions(positions);

			// Export each segment as a LINE
			for (int i = 0; i < positions.Length - 1; i++)
			{
				Vector3 start = positions[i];
				Vector3 end = positions[i + 1];

				sb.AppendLine("0");
				sb.AppendLine("LINE");
				sb.AppendLine("8");
				sb.AppendLine("0"); // Layer name
				sb.AppendLine("10");
				sb.AppendLine(start.x.ToString(CI));
				sb.AppendLine("20");
				sb.AppendLine(start.y.ToString(CI));
				sb.AppendLine("30");
				sb.AppendLine(start.z.ToString(CI));
				sb.AppendLine("11");
				sb.AppendLine(end.x.ToString(CI));
				sb.AppendLine("21");
				sb.AppendLine(end.y.ToString(CI));
				sb.AppendLine("31");
				sb.AppendLine(end.z.ToString(CI));
			}
		}

		sb.AppendLine("0");
		sb.AppendLine("ENDSEC");
		sb.AppendLine("0");
		sb.AppendLine("EOF");

		// Save file to Downloads folder
		string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads", "drawing.dxf");
		File.WriteAllText(downloadsPath, sb.ToString(), Encoding.UTF8);

		Debug.Log("?? DXF exported to: " + downloadsPath);
	}

	public void QuitApplication()
	{
		Debug.Log("?? Quitting Application");
		Application.Quit();
	}
}
