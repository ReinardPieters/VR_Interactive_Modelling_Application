using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PolygonTool : MonoBehaviour
{
    public Camera aimCamera;
    public PlaneProjector projector;
    public StrokeStore store;
    public SnapController snapCtrl;
    public InputActionProperty drawTrigger;
    public LaserLayer layer = LaserLayer.Cut;
    [Range(3, 128)] public int sides = 6;
    public float rotationDeg = 0f;

    bool placingCenter = false;
    Vector2 centerMM;

    void OnEnable() { drawTrigger.action.Enable(); }
    void OnDisable() { drawTrigger.action.Disable(); }

    void Update()
    {
        var ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (!projector.RayToPlane(ray, out _, out var mm)) return;
        var p = snapCtrl && snapCtrl.UseSnap ? store.Snap(mm) : mm;

        if (drawTrigger.action.WasPressedThisFrame())
        {
            centerMM = p; placingCenter = true;
        }
        else if (drawTrigger.action.WasReleasedThisFrame() && placingCenter)
        {
            placingCenter = false;
            float r = Vector2.Distance(centerMM, p);
            if (r < 0.1f || sides < 3) return;

            var pts = new List<Vector2>();
            float step = Mathf.PI * 2f / sides;
            float rot = rotationDeg * Mathf.Deg2Rad;
            for (int i = 0; i <= sides; i++)
                pts.Add(centerMM + new Vector2(Mathf.Cos(rot + i * step), Mathf.Sin(rot + i * step)) * r);

            store.polylines.Add(new PolylineMM { layer = layer, closed = true, ptsMM = pts });
        }
    }
}
