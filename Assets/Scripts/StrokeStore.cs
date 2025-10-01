using System.Collections.Generic;
using UnityEngine;

public enum LaserLayer { Cut, Score, Engrave }

[System.Serializable]
public class PolylineMM
{
    public LaserLayer layer;
    public bool closed;
    public List<Vector2> ptsMM = new List<Vector2>(); // points in millimetres
}

[CreateAssetMenu(fileName = "StrokeStore", menuName = "VR2D/StrokeStore")]
public class StrokeStore : ScriptableObject
{
    [Header("Snapping")]
    public float snapMM = 1f;
    public bool snapEnabledByDefault = true;

    [Header("Simplify")]
    public float rdpToleranceMM = 0.2f;

    [Header("Data")]
    public List<PolylineMM> polylines = new List<PolylineMM>();

    public Vector2 Snap(Vector2 mm)
    {
        float s = Mathf.Max(snapMM, 0.1f);
        return new Vector2(
            Mathf.Round(mm.x / s) * s,
            Mathf.Round(mm.y / s) * s
        );
    }

    // Ramer–Douglas–Peucker simplification (2D)
    public static void SimplifyRDP(List<Vector2> pts, float eps, List<Vector2> outPts)
    {
        outPts.Clear();
        if (pts == null || pts.Count <= 2)
        {
            if (pts != null) outPts.AddRange(pts);
            return;
        }

        var stack = new Stack<System.ValueTuple<int, int>>();
        int first = 0, last = pts.Count - 1;
        var keep = new bool[pts.Count];
        keep[first] = keep[last] = true;
        stack.Push(new System.ValueTuple<int, int>(first, last));

        while (stack.Count > 0)
        {
            var segment = stack.Pop();
            int a = segment.Item1;
            int b = segment.Item2;

            float maxD = -1f;
            int idx = -1;
            Vector2 A = pts[a], B = pts[b];
            Vector2 AB = B - A;
            float abLen = AB.magnitude;

            for (int i = a + 1; i < b; i++)
            {
                float d;
                if (abLen < Mathf.Epsilon) d = Vector2.Distance(pts[i], A);
                else
                {
                    // distance from point to line
                    float cross = Mathf.Abs((pts[i].x - A.x) * AB.y - (pts[i].y - A.y) * AB.x);
                    d = cross / abLen;
                }
                if (d > maxD) { maxD = d; idx = i; }
            }

            if (maxD > eps)
            {
                keep[idx] = true;
                stack.Push(new System.ValueTuple<int, int>(a, idx));
                stack.Push(new System.ValueTuple<int, int>(idx, b));
            }
        }

        for (int i = 0; i < keep.Length; i++)
            if (keep[i]) outPts.Add(pts[i]);
    }
}
