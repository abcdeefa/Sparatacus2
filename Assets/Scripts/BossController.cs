using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Ω∫≈»")]
    public int maxHp = 300;
    public float speed = 1.6f;

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
        if (hp <= 0)
        {
            if (spawner != null) spawner.OnEnemyKilled(gameObject);
            Destroy(gameObject);
        }
    }

    IEnumerator Flash()
    {
        if (sr == null) yield break;
        var orig = sr.color;
        sr.color = new Color(1f, 0.5f, 0.5f);
        yield return new WaitForSeconds(0.06f);
        sr.color = orig;
    }
}
