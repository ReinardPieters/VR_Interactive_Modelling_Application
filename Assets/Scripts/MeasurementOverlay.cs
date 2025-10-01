using UnityEngine;
using TMPro;

public class MeasurementOverlay : MonoBehaviour
{
    public TextMeshProUGUI label;      // world-space canvas or screen-space
    public RectTransform follow;       // optional: place near controller reticle

    static MeasurementOverlay _inst;
    void Awake() { _inst = this; Hide(); }

    public static void ShowLine(float mm)
    {
        if (!_inst || !_inst.label) return;
        _inst.label.text = $"Length: {mm:F1} mm";
        _inst.label.gameObject.SetActive(true);
    }

    public static void ShowCircle(float radiusMM)
    {
        if (!_inst || !_inst.label) return;
        _inst.label.text = $"Radius: {radiusMM:F1} mm\nDiameter: {(radiusMM * 2f):F1} mm";
        _inst.label.gameObject.SetActive(true);
    }

    public static void ShowPolygon(float edgeMM, float perimeterMM)
    {
        if (!_inst || !_inst.label) return;
        _inst.label.text = $"Edge: {edgeMM:F1} mm\nPerimeter: {perimeterMM:F1} mm";
        _inst.label.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if (_inst && _inst.label) _inst.label.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (follow) label.transform.position = follow.position;
    }
}
