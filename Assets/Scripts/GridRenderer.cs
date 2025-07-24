using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform drawingPlane;
    public float majorStep = 0.10f; // 10 cm
    public float minorStep = 0.05f; // 5 cm

    [Header("Line Prefabs")]
    public GameObject majorLinePrefab;
    public GameObject minorLinePrefab;

    private Transform gridParent;

    public void GenerateGrid()
    {
        if (gridParent != null)
            Destroy(gridParent.gameObject);

        gridParent = new GameObject("GridParent").transform;
        gridParent.SetParent(drawingPlane, false); // Stay local
        gridParent.localPosition = Vector3.zero;
        gridParent.localRotation = Quaternion.identity;
        gridParent.localScale = Vector3.one;

        float width = drawingPlane.lossyScale.x;
        float height = drawingPlane.lossyScale.y;
        float zOffset = -0.01f;

        // Vertical lines (constant X)
        for (float x = 0f; x <= width; x += minorStep)
        {
            bool isMajor = Mathf.Abs((x * 100f) % (majorStep * 100f)) < 0.01f;

            Vector3 start = new Vector3(x - width / 2f, -height / 2f, zOffset);
            Vector3 end   = new Vector3(x - width / 2f,  height / 2f, zOffset);

            DrawLine(start, end, isMajor ? majorLinePrefab : minorLinePrefab);
        }

        // Horizontal lines (constant Y)
        for (float y = 0f; y <= height; y += minorStep)
        {
            bool isMajor = Mathf.Abs((y * 100f) % (majorStep * 100f)) < 0.01f;

            Vector3 start = new Vector3(-width / 2f, y - height / 2f, zOffset);
            Vector3 end   = new Vector3( width / 2f, y - height / 2f, zOffset);

            DrawLine(start, end, isMajor ? majorLinePrefab : minorLinePrefab);
        }
    }

    private void DrawLine(Vector3 localStart, Vector3 localEnd, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(gridParent);

        LineRenderer lr = obj.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;

        Vector3 worldStart = drawingPlane.TransformPoint(localStart);
        Vector3 worldEnd   = drawingPlane.TransformPoint(localEnd);

        lr.positionCount = 2;
        lr.SetPosition(0, worldStart);
        lr.SetPosition(1, worldEnd);
    }
}
