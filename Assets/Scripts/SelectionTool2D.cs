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

    [Header("Input (bind these in Inspector)")]
    // Press to select nearest polyline
    public InputActionProperty selectTrigger;   // Button / Press
    // Press to commit transform (creates a snapshot for undo/redo)
    public InputActionProperty commitGrip;      // Button / Press
    // Left stick (Vector2): move selection in mm
    public InputActionProperty moveAxis;        // Value / Vector2
    // Right stick X: rotate selection (deg/sec)
    public InputActionProperty rotateAxisX;     // Value / Axis (1D)
    // Right stick Y: uniform scale (per second)
    public InputActionProperty scaleAxisY;      // Value / Axis (1D)

    [Header("Tuning (mm, deg)")]
    public float selectMaxDistMM = 8f;
    public float moveSpeedMMPerSec = 100f;
    public float rotSpeedDegPerSec = 90f;
    public float scalePerSec = 0.5f;

    // --- internal state
    int selectedIndex = -1;
    Vector2 pivotMM; // centroid of selected polyline
    bool HasSelection => (store && selectedIndex >= 0 && selectedIndex < store.polylines.Count);

    void OnEnable()
    {
        if (selectTrigger.reference != null) selectTrigger.action.Enable();
        if (commitGrip.reference != null) commitGrip.action.Enable();
        if (moveAxis.reference != null) moveAxis.action.Enable();
        if (rotateAxisX.reference != null) rotateAxisX.action.Enable();
        if (scaleAxisY.reference != null) scaleAxisY.action.Enable();
    }

    void OnDisable()
    {
        if (selectTrigger.reference != null) selectTrigger.action.Disable();
        if (commitGrip.reference != null) commitGrip.action.Disable();
        if (moveAxis.reference != null) moveAxis.action.Disable();
        if (rotateAxisX.reference != null) rotateAxisX.action.Disable();
        if (scaleAxisY.reference != null) scaleAxisY.action.Disable();
    }

    void Update()
    {
        if (!aimCamera || !projector || !store) return;

        // --- SELECT ---
        if (selectTrigger.reference != null && selectTrigger.action.WasPressedThisFrame())
        {
            var ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            if (projector.RayToPlane(ray, out _, out var hitMM))
            {
                selectedIndex = FindNearestPolyline(hitMM, selectMaxDistMM);
                if (HasSelection) pivotMM = ComputeCentroid(store.polylines[selectedIndex].ptsMM);
            }
        }

        if (!HasSelection) return;

        // --- MOVE ---
        if (moveAxis.reference != null)
        {
            Vector2 mv = moveAxis.action.ReadValue<Vector2>(); // [-1..1]
            if (mv.sqrMagnitude > 0.0001f)
            {
                Vector2 delta = mv * (moveSpeedMMPerSec * Time.deltaTime);
                TranslateSelected(delta);
                pivotMM += delta;
            }
        }

        // --- ROTATE ---
        if (rotateAxisX.reference != null)
        {
            float rx = rotateAxisX.action.ReadValue<float>(); // [-1..1]
            if (Mathf.Abs(rx) > 0.001f)
            {
                float deg = rx * rotSpeedDegPerSec * Time.deltaTime;
                RotateSelected(pivotMM, deg);
            }
        }

        // --- SCALE (uniform) ---
        if (scaleAxisY.reference != null)
        {
            float sy = scaleAxisY.action.ReadValue<float>(); // [-1..1]
            if (Mathf.Abs(sy) > 0.001f)
            {
                float s = 1f + sy * (scalePerSec * Time.deltaTime);
                ScaleSelected(pivotMM, s);
            }
        }

        // --- COMMIT (snapshot for Undo/Redo) ---
        if (commitGrip.reference != null && commitGrip.action.WasPressedThisFrame() && history)
        {
            history.PushSnapshot();
            Debug.Log("Selection committed (snapshot).");
        }
    }

    // ---------- helpers ----------
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
        float ab2 = Vector2.Dot(ab, ab);
        if (ab2 < Mathf.Epsilon) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab2);
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
