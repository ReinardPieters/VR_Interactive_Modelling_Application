using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResizeBoardUI : MonoBehaviour
{
    public GameObject resizePanel;
    public Button openButton;
    public Button confirmButton;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Transform drawingPlane;

    void Start()
    {
        resizePanel.SetActive(false);
        openButton.onClick.AddListener(() => resizePanel.SetActive(true));
        confirmButton.onClick.AddListener(ApplyResize);
    }

    void ApplyResize()
    {
        float widthCM = float.Parse(widthInput.text);
        float heightCM = float.Parse(heightInput.text);

        // Convert cm to meters for Unity scale
        float widthM = widthCM / 100f;
        float heightM = heightCM / 100f;

        drawingPlane.localScale = new Vector3(widthM, 1f, heightM);

        resizePanel.SetActive(false);
    }
}
