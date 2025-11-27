using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class GameSceneBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "SampleScene") return;
        if (FindObjectOfType<GameSceneBootstrap>() != null) return;
        new GameObject("GameSceneBootstrap").AddComponent<GameSceneBootstrap>();
    }

    void Start()
    {
        Time.timeScale = 1f;
        Build();
        Destroy(gameObject);
    }

    void Build()
    {
        PlayerController2D player = FindPlayer();
        if (player != null)
        {
            player.gameObject.SetActive(true);
            SetupPlayer(player);
        }

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (waveManager != null && waveManager.spawner == null && spawner != null)
            waveManager.spawner = spawner;

        SetupBackground(waveManager);
        BuildUI(player, waveManager);
        EnsureEventSystem();
    }

    PlayerController2D FindPlayer()
    {
        PlayerController2D[] players;
#if UNITY_2020_1_OR_NEWER
        players = FindObjectsOfType<PlayerController2D>(true);
#else
        players = FindObjectsOfType<PlayerController2D>();
#endif
        if (players == null || players.Length == 0) return null;

        PlayerController2D chosen = null;
        foreach (var p in players)
        {
            if (p == null) continue;
            if (p.isActiveAndEnabled)
            {
                chosen = p;
                break;
            }
            if (chosen == null) chosen = p;
        }

        foreach (var p in players)
        {
            if (p != null && p != chosen && p.gameObject.activeSelf)
                p.gameObject.SetActive(false);
        }

        return chosen;
    }

    void SetupPlayer(PlayerController2D player)
    {
        player.tag = "Player";
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            foreach (var t in player.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = playerLayer;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (player.attackOrigin == null)
        {
            var origin = new GameObject("attackorigin").transform;
            origin.SetParent(player.transform);
            origin.localPosition = new Vector3(0.3f, 0f, 0f);
            player.attackOrigin = origin;
        }

        int enemyMask = LayerMask.GetMask("Enemy");
        player.enemyLayer = enemyMask == 0 ? Physics2D.AllLayers : enemyMask;
    }

    void SetupBackground(WaveManager waveManager)
    {
        GameObject bg = GameObject.Find("content_0") ?? GameObject.Find("content");
        if (bg == null) return;

        var bm = bg.GetComponent<BackgroundManager>();
        if (bm == null) bm = bg.AddComponent<BackgroundManager>();

        bm.sr = bg.GetComponent<SpriteRenderer>();
        bm.waveManager = waveManager;
    }

    void BuildUI(PlayerController2D player, WaveManager waveManager)
    {
        Canvas canvas = CreateCanvas();
        var ui = canvas.gameObject.AddComponent<GameUIController>();
        ui.player = player;
        ui.waveManager = waveManager;

        // HP Bar
        var hpRoot = CreatePanel(canvas.transform, "HPBar", new Vector2(0, 1), new Vector2(0, 1), new Vector2(220, 26), new Vector2(120, -40), new Color(0f, 0f, 0f, 0.5f));
        var hpFill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        hpFill.transform.SetParent(hpRoot, false);
        var hpFillRect = hpFill.GetComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        hpFillRect.anchorMax = Vector2.one;
        hpFillRect.offsetMin = new Vector2(2, 2);
        hpFillRect.offsetMax = new Vector2(-2, -2);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.color = new Color(0.86f, 0.13f, 0.13f, 0.95f);

        var healthUI = hpRoot.gameObject.AddComponent<PlayerHealthUI>();
        healthUI.player = player;
        healthUI.hpFill = hpFill;
        ui.healthUI = healthUI;

        // Wave text
        var waveText = CreateTMP(canvas.transform, "WaveText", "Wave 1 / 10", 28, TextAlignmentOptions.Center);
        var waveRect = waveText.GetComponent<RectTransform>();
        waveRect.anchorMin = new Vector2(0.5f, 1f);
        waveRect.anchorMax = new Vector2(0.5f, 1f);
        waveRect.anchoredPosition = new Vector2(0, -32);
        waveRect.sizeDelta = new Vector2(260, 40);
        ui.waveText = waveText;

        // Game Over panel
        Button restartBtn;
        Button titleBtn;
        var gameOverPanel = CreateMessagePanel(canvas.transform, "GameOverPanel", "Game Over", "Restart", "Title", out restartBtn, out titleBtn);
        gameOverPanel.SetActive(false);
        ui.gameOverPanel = gameOverPanel;
        ui.restartButton = restartBtn;
        ui.toTitleButton = titleBtn;

        // Victory panel
        Button victoryBtn;
        Button victoryTitleBtn;
        var victoryPanel = CreateMessagePanel(canvas.transform, "VictoryPanel", "Victory", "Back to Title", "Restart", out victoryBtn, out victoryTitleBtn);
        victoryPanel.SetActive(false);
        ui.victoryPanel = victoryPanel;
        ui.victoryButton = victoryBtn;
        ui.victoryTitleButton = victoryTitleBtn;
    }

    Canvas CreateCanvas()
    {
        var go = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return canvas;
    }

    RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        go.GetComponent<Image>().color = color;
        return rect;
    }

    TextMeshProUGUI CreateTMP(Transform parent, string name, string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    GameObject CreateMessagePanel(Transform parent, string name, string title, string primaryButton, string secondaryButton, out Button primary, out Button secondary)
    {
        var panel = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420, 230), Vector2.zero, new Color(0f, 0f, 0f, 0.75f)).gameObject;

        var titleTmp = CreateTMP(panel.transform, "Title", title, 36, TextAlignmentOptions.Center);
        var titleRect = titleTmp.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(360, 60);

        primary = CreateButton(panel.transform, "PrimaryButton", primaryButton, new Vector2(0, -100));
        secondary = CreateButton(panel.transform, "SecondaryButton", secondaryButton, new Vector2(0, -150));

        return panel;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var tmp = CreateTMP(go.transform, "Label", label, 24, TextAlignmentOptions.Center);
        var tmpRect = tmp.GetComponent<RectTransform>();
        tmpRect.anchorMin = Vector2.zero;
        tmpRect.anchorMax = Vector2.one;
        tmpRect.offsetMin = Vector2.zero;
        tmpRect.offsetMax = Vector2.zero;

        return go.GetComponent<Button>();
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
