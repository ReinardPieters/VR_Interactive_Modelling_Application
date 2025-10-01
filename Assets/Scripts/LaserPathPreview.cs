using System.Collections.Generic;
using UnityEngine;

public class LaserPathPreview : MonoBehaviour
{
    [Header("Data")]
    public StrokeStore store;

    [Header("Rendering")]
    public Material lineMaterial; // emission-capable (Unlit with Emission on)
    public float lineWidth = 0.0012f; // ~1.2mm in world units if 1u=1m
    public Color cutColor = new(1f, 0f, 0f, 1f);
    public Color scoreColor = new(0f, 0f, 1f, 1f);
    public Color engraveColor = new(0f, 0f, 0f, 1f);

    readonly List<LineRenderer> pool = new();

    void LateUpdate()
    {
        if (!store) return;

        // Ensure enough line renderers in pool
        int needed = store.polylines.Count;
        while (pool.Count < needed) pool.Add(CreateLR());
        for (int i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(i < needed);

        // Update each polyline
        for (int i = 0; i < store.polylines.Count; i++)
        {
            var pl = store.polylines[i];
            var lr = pool[i];
            lr.widthMultiplier = lineWidth;

            lr.startColor = lr.endColor = LayerColor(pl.layer);

            // Convert mm (store) to world positions on XZ plane (y=0)
            lr.positionCount = pl.ptsMM.Count;
            for (int k = 0; k < pl.ptsMM.Count; k++)
            {
                var pmm = pl.ptsMM[k];
                // Assuming drawing plane origin at world (0,0,0), x?x(mm), y?z(mm)
                // If your plane is elsewhere, parent this GO under the plane and scale by 0.001
                pool[i].transform.localScale = new Vector3(0.001f, 1f, 0.001f);
                lr.SetPosition(k, new Vector3(pmm.x, 0.002f, pmm.y)); // slight lift to avoid z-fight
            }
        }
    }

    LineRenderer CreateLR()
    {
        var go = new GameObject("LaserPathLine");
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.useWorldSpace = false;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    Color LayerColor(LaserLayer layer) => layer switch
    {
        LaserLayer.Cut => cutColor,
        LaserLayer.Score => scoreColor,
        _ => engraveColor
    };
}
