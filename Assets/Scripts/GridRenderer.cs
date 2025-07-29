using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform drawingPlane;
    public float majorStep = 10f; // 10 cm
    public float minorStep = 5f; // 5 cm

    [Header("Line Prefabs")]
    public GameObject majorLinePrefab;
    public GameObject minorLinePrefab;
    public GameObject labelPrefab;


    private Transform gridParent;

    // Scale factor to make board look big enough in VR
    // 1 real-world meter = 10 Unity units
    private const float worldScaleFactor = 10f; // 10x scale (1m → 10 units)


    public void GenerateGrid(float heightM, float widthM)
    {
        if (gridParent != null)
            Destroy(gridParent.gameObject);

        gridParent = new GameObject("GridParent").transform;
        gridParent.SetParent(drawingPlane, false);
        gridParent.localPosition = Vector3.zero;
        gridParent.localRotation = Quaternion.identity;
        gridParent.localScale = Vector3.one;

        Vector3 forwardOffset = new Vector3(0f, 0f, -0.01f);


        // Convert steps from cm to meters
        float minorStepM = minorStep;
        float majorStepM = majorStep;

        int verticalLines = Mathf.RoundToInt(widthM / minorStepM);
        int horizontalLines = Mathf.RoundToInt(heightM / minorStepM);
        int majorFrequency = Mathf.RoundToInt(majorStepM / minorStepM);

        // Vertical lines
        for (int i = 0; i <= verticalLines; i++)
        {
            float x = i * minorStepM;
            float normX = x - widthM / 2f;

            bool isMajor = (i % majorFrequency == 0);

            Vector3 start = new Vector3(normX, -heightM / 2f, 0f) + forwardOffset;
            Vector3 end   = new Vector3(normX, heightM / 2f, 0f) + forwardOffset;

            DrawLine(start, end, isMajor ? majorLinePrefab : minorLinePrefab);
        }

        // Horizontal lines
        for (int i = 0; i <= horizontalLines; i++)
        {
            float y = i * minorStepM;
            float normY = y - heightM / 2f;

            bool isMajor = (i % majorFrequency == 0);

            Vector3 start = new Vector3(-widthM / 2f, normY, 0f) + forwardOffset;
            Vector3 end   = new Vector3(widthM / 2f, normY, 0f) + forwardOffset;

            DrawLine(start, end, isMajor ? majorLinePrefab : minorLinePrefab);
        }

        PlaceLabel(heightM, widthM);
    }


    // Place label in bottom-right corner
    void PlaceLabel(float height, float width)
    {
        if (labelPrefab == null) return;

        GameObject label = Instantiate(labelPrefab, gridParent);
        
        float margin = 0.05f; // how far in from the corner (in meters)

        Vector3 localPos = new Vector3(width / 2f - margin, -height / 2f + margin, -0.01f);
        label.transform.localPosition = localPos;
        label.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    private void DrawLine(Vector3 localStart, Vector3 localEnd, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, gridParent);
        obj.transform.localPosition = Vector3.zero; // <- Forces local position


        LineRenderer lr = obj.GetComponent<LineRenderer>();
        lr.useWorldSpace = false;

        // Normalize for drawingPlane scale
        Vector3 inverseScale = new Vector3(
            1f / drawingPlane.localScale.x,
            1f / drawingPlane.localScale.y,
            1f / drawingPlane.localScale.z
        );

        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.Scale(localStart, inverseScale));
        lr.SetPosition(1, Vector3.Scale(localEnd, inverseScale));
    }


}
