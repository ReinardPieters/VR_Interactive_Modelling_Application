using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LinePreview : MonoBehaviour
{
    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = true;
    }

    public void Show(Vector3 a, Vector3 b)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, a + Vector3.up * 0.001f);
        lr.SetPosition(1, b + Vector3.up * 0.001f);
    }

    public void Hide() => lr.positionCount = 0;
}
