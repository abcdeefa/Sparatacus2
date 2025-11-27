using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform titleRect;
    public Button startButton;
    public RectTransform startRect;

    [Header("Pulse")]
    public float pulseSpeed = 3.5f;
    public float pulseScaleMin = 0.9f;
    public float pulseScaleMax = 1.15f;

    void Start()
    {
        if (startButton == null)
            startButton = GetComponentInChildren<Button>();
        if (startRect == null && startButton != null)
            startRect = startButton.GetComponent<RectTransform>();

        if (startButton != null)
            startButton.onClick.AddListener(() =>
                SceneManager.LoadScene("CharacterSelectScene"));
    }

    void Update()
    {
        if (startRect == null) return;
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f; // 0~1
        float s = Mathf.Lerp(pulseScaleMin, pulseScaleMax, t);
        startRect.localScale = new Vector3(s, s, 1f);
    }
}

