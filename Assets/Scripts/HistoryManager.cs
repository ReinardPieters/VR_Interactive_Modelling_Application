using System.Collections.Generic;
using UnityEngine;

public class HistoryManager : MonoBehaviour
{
    [Header("Data")]
    public StrokeStore store;

    [Header("Limits")]
    public int maxSnapshots = 50;

    readonly Stack<List<PolylineMM>> undoStack = new();
    readonly Stack<List<PolylineMM>> redoStack = new();

    void Awake()
    {
        if (!store) Debug.LogWarning("HistoryManager: StrokeStore not assigned.");
        Clear();
        // Initial baseline so first Undo does nothing harmful
        PushSnapshot();
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    public void PushSnapshot()
    {
        if (!store) return;
        var snap = DeepCopy(store.polylines);
        undoStack.Push(snap);
        if (undoStack.Count > maxSnapshots) _ = undoStack.ToArray(); // trim not strictly needed
        redoStack.Clear(); // invalidate redo on new action
    }

    public void Undo()
    {
        if (undoStack.Count <= 1 || !store) return; // keep at least one baseline
        // Current state ? redo
        var cur = DeepCopy(store.polylines);
        redoStack.Push(cur);

        // Pop previous from undo ? apply
        _ = undoStack.Pop();
        var prev = DeepCopy(undoStack.Peek());
        store.polylines = prev;
        Debug.Log("Undo: restored previous snapshot. Count=" + store.polylines.Count);
    }

    public void Redo()
    {
        if (redoStack.Count == 0 || !store) return;
        var next = DeepCopy(redoStack.Pop());
        // Push current to undo
        undoStack.Push(DeepCopy(store.polylines));
        // Apply next
        store.polylines = next;
        Debug.Log("Redo: applied next snapshot. Count=" + store.polylines.Count);
    }

    // Utility: deep copy of polylines
    static List<PolylineMM> DeepCopy(List<PolylineMM> src)
    {
        var dst = new List<PolylineMM>(src.Count);
        foreach (var pl in src)
        {
            var copy = new PolylineMM { layer = pl.layer, closed = pl.closed, ptsMM = new List<Vector2>(pl.ptsMM) };
            dst.Add(copy);
        }
        return dst;
    }
}
