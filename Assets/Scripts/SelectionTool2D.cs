using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionTool2D : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCamera;
    public PlaneProjector projector;
    public StrokeStore store;
    public HistoryManager history;

    [Header("Input")]
    public InputActionProperty selectTrigger;     
    public InputActionProperty commitGrip;       
    public InputActionProperty moveAxis;          
    public InputActionProperty rotateAxisX;      
    public InputActionProperty scaleAxisY;        

    [Header("Tuning (mm, deg)")]
    public float selectMaxDistMM = 8f;
    public float moveSpeedMMPerSec = 100f;
    public float rotSpeedDegPerSec = 90f;
    public float scalePerSec = 0.5f;

    int selectedIndex = -1;
    Vector2 pivotMM; 
    bool hasSelection => (selectedIndex >= 0 && selectedIndex < store.polylines.Count);

    void OnEnable()
    {
        selectTrigger.action.Enable();
        commitGrip.action.Enable();
        moveAxis.action.Enable();
        rotateAxisX.action.Enable();
        scaleAxisY.action.Enable();
    }
    void OnDisable()
    {
        selectTrigger.action.Disable();
        commitGrip.action.Disable();
        moveAxis.action.Disable();
        rotateAxisX.action.Disable();
        scaleAxisY.action.Disable();
    }

    void Update()
    {
        if (aimCamera == null || projector == null || store == null) return;

        // Selection
        if (selectTrigger.action.WasPressedThisFrame())
        {
            var ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            if (projector.RayToPlane(ray, out _, out var hitMM))
            {
                selectedIndex = FindNearestPolyline(hitMM, selectMaxDistMM);
                if (hasSelection) pivotMM = ComputeCentroid(store.polylines[selectedIndex].ptsMM);
            }
        }

        if (!hasSelection) return;

        // Move
        var mv = moveAxis.action.ReadValue<Vector2>(); // x,y [-1..1]
        if (mv.sqrMagnitude > 0.0001f)
        {
            Vector2 delta = mv * (moveSpeedMMPerSec * Time.deltaTime);
            TranslateSelected(delta);
            pivotMM += delta;
        }

        // Rotate
        float rx = rotateAxisX.action.ReadValue<float>(); // [-1..1]
        if (Mathf.Abs(rx) > 0.001f)
        {
            float deg = rx * rotSpeedDegPerSec * Time.deltaTime;
            RotateSelected(pivotMM, deg);
        }

        // Scale (uniform)
        float sy = scaleAxisY.action.ReadValue<float>(); // [-1..1]
        if (Mathf.Abs(sy) > 0.001f)
        {
            float s = 1f + sy * (scalePerSec * Time.deltaTime);
            ScaleSelected(pivotMM, s);
        }

        // Commit
        if (commitGrip.action.WasPressedThisFrame() && history)
        {
            history.PushSnapshot();
            Debug.Log("Selection committed (snapshot).");
        }
    }

    int FindNearestPolyline(Vector2 mm, float maxDistMM)
    {
        int idx = -1; float best = maxDistMM;
        for (int i = 0; i < store.polylines.Count; i++)
        {
            var pl = store.polylines[i];
            float d = DistancePointToPolyline(mm, pl.ptsMM);
            if (d < best) { best = d; idx = i; }
        }
        return idx;
    }

    static float DistancePointToPolyline(Vector2 p, List<Vector2> pts)
    {
        if (pts == null || pts.Count < 2) return float.MaxValue;
        float best = float.MaxValue;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            float d = DistancePointToSegment(p, pts[i], pts[i + 1]);
            if (d < best) best = d;
        }
        return best;
    }

    static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return Vector2.Distance(p, a + t * ab);
    }

    static Vector2 ComputeCentroid(List<Vector2> pts)
    {
        if (pts == null || pts.Count == 0) return Vector2.zero;
        float x = 0, y = 0;
        foreach (var v in pts) { x += v.x; y += v.y; }
        return new Vector2(x / pts.Count, y / pts.Count);
    }

    void TranslateSelected(Vector2 delta)
    {
        var pl = store.polylines[selectedIndex];
        for (int i = 0; i < pl.ptsMM.Count; i++) pl.ptsMM[i] += delta;
    }

    void RotateSelected(Vector2 pivot, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        var pl = store.polylines[selectedIndex];
        for (int i = 0; i < pl.ptsMM.Count; i++)
        {
            var v = pl.ptsMM[i] - pivot;
            var vr = new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
            pl.ptsMM[i] = pivot + vr;
        }
    }

    void ScaleSelected(Vector2 pivot, float scale)
    {
        var pl = store.polylines[selectedIndex];
        for (int i = 0; i < pl.ptsMM.Count; i++)
            pl.ptsMM[i] = pivot + (pl.ptsMM[i] - pivot) * scale;
    }
}
