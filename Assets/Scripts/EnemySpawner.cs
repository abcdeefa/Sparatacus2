using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct WaveConfig
{
    public int enemyCount;        // 이번 웨이브에 "총 몇 마리"를 소환할지
    public float spawnInterval;   // 소환 간격
    public bool spawnBoss;        // 보스 포함 여부
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;

    [Header("Spawn Point (한쪽)")]
    public Transform spawnPoint;

    // ---- 런타임 상태 ----
    private readonly List<GameObject> alive = new List<GameObject>();
    private int toSpawnTotal;     // 이번 웨이브에 소환해야 할 '총 수'
    private int spawned;          // 지금까지 소환한 수
    private bool bossQueued;      // 보스를 이 웨이브에 소환해야 하는지
    private bool bossSpawned;     // 보스를 실제로 소환했는지
    private bool spawning;        // 소환 코루틴 동작 중

    // 이벤트(옵션): UI 등에 연결해서 사용 가능
    public System.Action<int, int> OnSpawnedChanged; // (spawned, toSpawnTotal)
    public System.Action OnWaveAllSpawned;          // 이번 웨이브 '모두' 소환 완료
    public System.Action OnWaveCleared;             // 소환도 끝났고, 생존도 0 → 전멸

    // ---- 외부에서 호출 ----
    public void BeginWave(WaveConfig cfg)
    {
        StopAllCoroutines();
        alive.Clear();

        toSpawnTotal = Mathf.Max(0, cfg.enemyCount);
        spawned = 0;
        bossQueued = cfg.spawnBoss;
        bossSpawned = false;
        spawning = true;

        StartCoroutine(SpawnRoutine(cfg));
    }

    public bool IsWaveSpawning() => spawning;              // 아직 소환 중인가?
    public bool IsWaveAllSpawned() => spawned >= toSpawnTotal && (!bossQueued || bossSpawned);
    public bool HasAliveEnemies()
    {
        alive.RemoveAll(a => a == null);
        return alive.Count > 0;
    }
    public bool IsWaveCleared() => IsWaveAllSpawned() && !HasAliveEnemies();

    // ---- 내부 구현 ----
    IEnumerator SpawnRoutine(WaveConfig cfg)
    {
        // 1) 잡몹 제한 수 만큼만 소환
        while (spawned < toSpawnTotal)
        {
            SpawnEnemy();
            OnSpawnedChanged?.Invoke(spawned, toSpawnTotal);
            yield return new WaitForSeconds(cfg.spawnInterval);
        }

        // 2) 보스 소환(해당 웨이브라면 한 번만)
        if (bossQueued && !bossSpawned)
        {
            yield return new WaitForSeconds(Mathf.Max(0.5f, cfg.spawnInterval));
            SpawnBoss();
        }

        spawning = false;
        OnWaveAllSpawned?.Invoke();
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoint == null)
        {
            Debug.LogError("[Spawner] Enemy Prefab 또는 SpawnPoint 누락");
            return;
        }

        var e = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        // 가시성(배경 위)에 문제 없게 간단 세팅
        var sr = e.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.sortingOrder = 10; }
        e.transform.localScale = Vector3.one * 2.1f;

        alive.Add(e);
        spawned++;
    }

    void SpawnBoss()
    {
        if (bossPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("[Spawner] Boss Prefab 또는 SpawnPoint 누락(보스 생략)");
            bossSpawned = true; // 누락되어도 웨이브 진행 막히지 않게
            return;
        }

        var b = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
        var sr = b.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.sortingOrder = 11; }
        b.transform.localScale = Vector3.one * 1.2f;

        alive.Add(b);
        bossSpawned = true;
    }

    // Enemy/Boss에서 죽을 때 호출해 주세요 (이미 그렇게 쓰고 계시면 그대로 동작)
    public void OnEnemyKilled(GameObject who)
    {
        alive.Remove(who);
        if (IsWaveCleared())
            OnWaveCleared?.Invoke();
    }

    // 씬에서 스폰 위치 확인용(선택)
    void OnDrawGizmos()
    {
        if (spawnPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
    }
}
