using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("스탯")]
    public int maxHp = 40;
    public float speed = 2f;

    private int hp;
    private Transform target;
    private EnemySpawner spawner;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        spawner = FindObjectOfType<EnemySpawner>();
    }

    void Start()
    {
        hp = maxHp;
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) target = playerGo.transform;
        else Debug.LogWarning("[Enemy] Player 태그 오브젝트가 없어요.");
    }

    void Update()
    {
        if (hp <= 0) return;
        if (target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    public void TakeDamage(int dmg)
    {
        hp -= Mathf.Max(1, dmg);
        if (sr != null) StartCoroutine(Flash());

        Debug.Log($"[Enemy Hit] {gameObject.name} : -{dmg} 데미지 (남은 HP: {hp})");

        if (hp <= 0)
        {
            Debug.Log($"[Enemy Dead] {gameObject.name} 처치됨");
            if (spawner != null) spawner.OnEnemyKilled(gameObject);
            Destroy(gameObject);
        }
    }


    IEnumerator Flash()
    {
        if (sr == null) yield break;
        var orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.06f);
        sr.color = orig;
    }
}
