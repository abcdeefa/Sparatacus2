using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController2D player;
    public WaveManager waveManager;
    public PlayerHealthUI healthUI;
    public TextMeshProUGUI waveText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Buttons")]
    public Button restartButton;
    public Button toTitleButton;
    public Button victoryButton;
    public Button victoryTitleButton;

    void Start()
    {
        if (healthUI != null && player != null)
            healthUI.player = player;

        HidePanels();
        HookButtons();

        if (player != null)
            player.onDeath += OnPlayerDeath;

        if (waveManager != null)
        {
            waveManager.OnWaveStarted += UpdateWaveText;
            waveManager.OnWaveCompleted += UpdateWaveText;
            waveManager.OnAllWavesCleared += OnAllWavesCleared;
            UpdateWaveText(waveManager.CurrentWaveNumber);
        }
    }

    void HookButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("SampleScene");
            });
        }

        if (toTitleButton != null)
        {
            toTitleButton.onClick.RemoveAllListeners();
            toTitleButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("Title");
            });
        }

        if (victoryButton != null)
        {
            victoryButton.onClick.RemoveAllListeners();
            victoryButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("Title");
            });
        }

        if (victoryTitleButton != null)
        {
            victoryTitleButton.onClick.RemoveAllListeners();
            victoryTitleButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("SampleScene");
            });
        }
    }

    void HidePanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void OnPlayerDeath()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnAllWavesCleared()
    {
        if (player != null && player.CurrentHp <= 0) return;
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateWaveText(int waveIndex)
    {
        if (waveText == null || waveManager == null) return;
        int total = Mathf.Max(1, waveManager.TotalWaves);
        int waveNum = Mathf.Clamp(waveIndex, 1, total);
        waveText.text = $"Wave {waveNum} / {total}";
    }

    void OnDestroy()
    {
        if (player != null)
            player.onDeath -= OnPlayerDeath;
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= UpdateWaveText;
            waveManager.OnWaveCompleted -= UpdateWaveText;
            waveManager.OnAllWavesCleared -= OnAllWavesCleared;
        }
    }
}
