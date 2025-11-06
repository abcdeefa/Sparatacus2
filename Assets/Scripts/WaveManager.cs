using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;

    [Header("웨이브 간 대기(전멸 후 다음 웨이브까지)")]
    public float timeBetweenWaves = 2f;

    [Header("웨이브 구성(직접 입력 가능)")]
    public List<WaveConfig> waves = new List<WaveConfig>(); // 인스펙터에서 크기 늘리고 각 웨이브 입력

    [Header("자동 생성 옵션(수동 입력이 비어있을 때만 사용)")]
    public int autoWaveCount = 10;
    public int baseCount = 5;
    public int addPerWave = 1;
    public float baseInterval = 1.5f;
    public float intervalDecay = 0.1f; // 웨이브마다 조금씩 빨라짐(최소 0.5)

    int currentWaveIndex = -1;
    bool running;

    void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("WaveManager: Spawner 연결이 필요합니다.");
            return;
        }

        // 웨이브가 비어있으면 자동 생성 (5,10 웨이브 보스)
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

            Debug.Log($"[WAVE {i + 1}] 시작: {cfg.enemyCount}마리 / 간격 {cfg.spawnInterval:0.00}s / 보스:{cfg.spawnBoss}");
            spawner.BeginWave(cfg);

            // 1) 모든 소환이 끝날 때까지 대기
            yield return new WaitUntil(() => spawner.IsWaveAllSpawned());

            // 2) 전멸(생존 0) 될 때까지 대기
            yield return new WaitUntil(() => spawner.IsWaveCleared());

            Debug.Log($"[WAVE {i + 1}] 클리어!");
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        running = false;
        Debug.Log("모든 웨이브 완료!");
    }

    // (옵션) 외부 UI에서 현재 웨이브 번호를 얻고 싶을 때 사용
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public bool IsRunning => running;
}
