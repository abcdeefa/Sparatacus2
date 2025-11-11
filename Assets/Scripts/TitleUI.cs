using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform titleRect;     // 상단 제목 Rect
    public Button startButton;          // START 버튼
    public RectTransform startRect;     // START 버튼 Rect(펄스용)

    [Header("Pulse")]
    public float pulseSpeed = 3.5f;
    public float pulseScaleMin = 0.9f;
    public float pulseScaleMax = 1.15f;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(() =>
                SceneManager.LoadScene("CharacterSelectScene")); // 다음 씬 이름
    }

    void Update()
    {
        if (startRect == null) return;
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f; // 0~1
        float s = Mathf.Lerp(pulseScaleMin, pulseScaleMax, t);
        startRect.localScale = new Vector3(s, s, 1f);
    }
}
