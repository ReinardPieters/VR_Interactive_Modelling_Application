using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircleTool : MonoBehaviour
{
    public Camera aimCamera;
    public PlaneProjector projector;
    public StrokeStore store;
    public SnapController snapCtrl;
    public InputActionProperty drawTrigger;
    public LaserLayer layer = LaserLayer.Cut;
    public int segments = 64;

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
            if (r < 0.1f) return;

            var pts = new List<Vector2>();
            float step = Mathf.PI * 2f / segments;
            for (int i = 0; i <= segments; i++)
                pts.Add(centerMM + new Vector2(Mathf.Cos(i * step), Mathf.Sin(i * step)) * r);

            store.polylines.Add(new PolylineMM { layer = layer, closed = true, ptsMM = pts });
        }
    }
}
