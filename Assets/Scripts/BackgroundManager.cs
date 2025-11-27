using UnityEngine;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite contentSprite;
    public Sprite afterSprite;
    public Sprite nightSprite;
    public WaveManager waveManager;
    public float fadeDuration = 0.5f;

    Coroutine fadeRoutine;

    void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        LoadMissingSprites();

        if (sr != null && contentSprite != null)
            sr.sprite = contentSprite;

        if (waveManager != null)
        {
            waveManager.OnWaveStarted += HandleWaveStarted;
            HandleWaveStarted(Mathf.Max(1, waveManager.CurrentWaveNumber));
        }
        else
        {
            HandleWaveStarted(1);
        }
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveStarted -= HandleWaveStarted;
    }

    void HandleWaveStarted(int wave)
    {
        if (sr == null) return;
        Sprite next = GetSpriteForWave(wave);
        Color tint = GetColorForWave(wave);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(next ?? sr.sprite, tint));
    }

    Sprite GetSpriteForWave(int wave)
    {
        if (wave <= 3) return contentSprite ?? sr?.sprite;
        if (wave <= 7) return afterSprite ?? contentSprite ?? sr?.sprite;
        return nightSprite ?? afterSprite ?? contentSprite ?? sr?.sprite;
    }

    Color GetColorForWave(int wave)
    {
        if (wave <= 3) return Color.white;
        if (wave <= 7) return new Color(0.95f, 0.9f, 0.9f, 1f);
        return new Color(0.8f, 0.9f, 1f, 1f);
    }

    IEnumerator FadeTo(Sprite next, Color targetColor)
    {
        if (sr == null || next == null) yield break;
        if (sr.sprite == next && (sr.color - targetColor).sqrMagnitude < 0.001f) yield break;

        Color startColor = sr.color;
        float startAlpha = startColor.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, a);
            yield return null;
        }

        sr.sprite = next;
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(targetColor.r, targetColor.g, targetColor.b, a);
            yield return null;
        }

        sr.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
    }

    void LoadMissingSprites()
    {
#if UNITY_EDITOR
        if (contentSprite == null)
            contentSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprite/content.png");
        if (afterSprite == null)
            afterSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprite/after.png");
        if (nightSprite == null)
            nightSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/sprite/night.png");
#endif
    }
}
