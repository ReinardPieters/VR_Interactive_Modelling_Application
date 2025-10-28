using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawingData
{
	public List<LineData> lines = new List<LineData>();
	public List<SphereData> spheres = new List<SphereData>();
}

[System.Serializable]
public class LineData
{
	public string type;
	public Vector3[] positions;
	public bool upperSemicircle;
}

[System.Serializable]
public class SphereData
{
	public Vector3 position;
	public float radius;
	public Color color;
	public string name;
}

public class CloudStorage : MonoBehaviour
{
	public Transform drawingQuadTransform;
	public GameObject linePrefab;
	public GameObject pointPrefab;

	// Serialize all lines and spheres
	public string SerializeDrawing()
	{
		DrawingData data = new DrawingData();

		foreach (Transform child in drawingQuadTransform)
		{
			// Save lines
			if (child.name.Contains("MajorLine"))
			{
				LineRenderer lr = child.GetComponent<LineRenderer>();
				if (lr == null) continue;

				LineData lineData = new LineData();
				lineData.positions = new Vector3[lr.positionCount];
				lr.GetPositions(lineData.positions);

				if (child.name.Contains("Arc")) lineData.type = "arc";
				else if (child.name.Contains("Parallel")) lineData.type = "parallel";
				else if (child.name.Contains("Circle")) lineData.type = "circle";
				else lineData.type = "line";

				lineData.upperSemicircle = child.name.Contains("Upper");

				data.lines.Add(lineData);
			}

			// Save spheres
			if (child.name.ToLower().Contains("sphere"))
			{
				SphereData sData = new SphereData();
				sData.position = child.position;
				sData.radius = child.localScale.x / 2f;

				Renderer rend = child.GetComponent<Renderer>();
				sData.color = rend != null ? rend.material.color : Color.white;

				sData.name = child.name;

				data.spheres.Add(sData);
			}
		}

		return JsonUtility.ToJson(data, true);
	}

	// Load lines and spheres from JSON
	public void LoadDrawing(string json)
	{
		DrawingData data = JsonUtility.FromJson<DrawingData>(json);

		// Clear current drawing
		foreach (Transform child in drawingQuadTransform)
			Destroy(child.gameObject);

		// Recreate lines
		foreach (var line in data.lines)
		{
			GameObject newLine = Instantiate(linePrefab, drawingQuadTransform);
			LineRenderer lr = newLine.GetComponent<LineRenderer>();
			lr.positionCount = line.positions.Length;
			lr.SetPositions(line.positions);

			newLine.name = "MajorLine_" + line.type;
			if (line.type == "circle" && line.upperSemicircle)
				newLine.name += "_Upper";
		}

		// Recreate spheres
		foreach (var sphere in data.spheres)
		{
			Vector3 spawnPos = new Vector3(sphere.position.x, sphere.position.y, drawingQuadTransform.position.z);
			GameObject newSphere = Instantiate(pointPrefab, spawnPos, Quaternion.identity);
			newSphere.transform.SetParent(drawingQuadTransform, true);
			newSphere.transform.localScale = Vector3.one * sphere.radius * 2f;

			Renderer rend = newSphere.GetComponent<Renderer>();
			if (rend != null)
				rend.material.color = sphere.color;

			newSphere.name = sphere.name;
		}

		Debug.Log($"? Drawing loaded successfully! ({data.lines.Count} lines, {data.spheres.Count} spheres)");
	}
}
