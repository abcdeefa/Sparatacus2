using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;

    [Header("Between waves")]
    public float timeBetweenWaves = 2f;

    [Header("Wave configs (can be left empty to auto generate)")]
    public List<WaveConfig> waves = new List<WaveConfig>();

    [Header("Auto generate settings")]
    public int autoWaveCount = 10;
    public int baseCount = 5;
    public int addPerWave = 1;
    public float baseInterval = 1.5f;
    public float intervalDecay = 0.1f;

    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action OnAllWavesCleared;

    int currentWaveIndex = -1;
    bool running;

    void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("WaveManager: Spawner reference missing");
            return;
        }

        if (waves.Count == 0)
        {
            for (int i = 0; i < autoWaveCount; i++)
            {
                var cfg = new WaveConfig
                {
                    enemyCount = baseCount + i * addPerWave,
                    spawnInterval = Mathf.Max(0.5f, baseInterval - i * intervalDecay),
                    spawnBoss = (i + 1 == 5 || i + 1 == 10)
                };
                waves.Add(cfg);
            }
        }

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        running = true;

        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            var cfg = waves[i];

            Debug.Log($"[WAVE {i + 1}] start: {cfg.enemyCount} mobs / interval {cfg.spawnInterval:0.00}s / boss:{cfg.spawnBoss}");
            OnWaveStarted?.Invoke(CurrentWaveNumber);
            spawner.BeginWave(cfg);

            yield return new WaitUntil(() => spawner.IsWaveAllSpawned());
            yield return new WaitUntil(() => spawner.IsWaveCleared());

            Debug.Log($"[WAVE {i + 1}] cleared");
            OnWaveCompleted?.Invoke(CurrentWaveNumber);
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        running = false;
        Debug.Log("All waves finished!");
        OnAllWavesCleared?.Invoke();
    }

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public bool IsRunning => running;
    public int TotalWaves => waves.Count;
}

