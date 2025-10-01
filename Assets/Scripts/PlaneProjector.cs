using UnityEngine;

public class PlaneProjector : MonoBehaviour
{
    public Transform drawingPlane; // Assign the bed/grid transform (the Quad)

    const float M_TO_MM = 1000f;

    // Ray -> plane intersection -> returns hit in mm coordinates on the bed
    public bool RayToPlane(Ray ray, out Vector3 worldHit, out Vector2 mm)
    {
        worldHit = Vector3.zero;
        mm = Vector2.zero;
        if (!drawingPlane) return false;

        var plane = new Plane(drawingPlane.up, drawingPlane.position);
        if (!plane.Raycast(ray, out float t)) return false;

        worldHit = ray.GetPoint(t);
        var local = drawingPlane.InverseTransformPoint(worldHit);

        // Assume local XZ is the bed; drawingPlane.localScale is in meters
        float halfWm = drawingPlane.localScale.x * 0.5f;
        float halfHm = drawingPlane.localScale.z * 0.5f;

        float xm = Mathf.Clamp(local.x, -halfWm, halfWm);
        float zm = Mathf.Clamp(local.z, -halfHm, halfHm);

        mm = new Vector2((xm + halfWm) * M_TO_MM, (zm + halfHm) * M_TO_MM);
        return true;
    }
}
