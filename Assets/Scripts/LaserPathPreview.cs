using System.Collections.Generic;
using UnityEngine;

public class LaserPathPreview : MonoBehaviour
{
    [Header("Data")]
    public StrokeStore store;

    [Header("Rendering")]
    public Material lineMaterial; 
    public float lineWidth = 0.0012f; 
    public Color cutColor = new(1f, 0f, 0f, 1f);
    public Color scoreColor = new(0f, 0f, 1f, 1f);
    public Color engraveColor = new(0f, 0f, 0f, 1f);

    readonly List<LineRenderer> pool = new();

    void LateUpdate()
    {
        if (!store) return;

        
        int needed = store.polylines.Count;
        while (pool.Count < needed) pool.Add(CreateLR());
        for (int i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(i < needed);

       
        for (int i = 0; i < store.polylines.Count; i++)
        {
            var pl = store.polylines[i];
            var lr = pool[i];
            lr.widthMultiplier = lineWidth;

            lr.startColor = lr.endColor = LayerColor(pl.layer);

            
            lr.positionCount = pl.ptsMM.Count;
            for (int k = 0; k < pl.ptsMM.Count; k++)
            {
                var pmm = pl.ptsMM[k];
            
                pool[i].transform.localScale = new Vector3(0.001f, 1f, 0.001f);
                lr.SetPosition(k, new Vector3(pmm.x, 0.002f, pmm.y)); 
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
