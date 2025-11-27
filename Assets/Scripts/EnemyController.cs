using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 40;
    public float speed = 2f;
    public int damage = 10;
    public float attackRange = 0.7f;
    public float attackCooldown = 1f;

    int hp;
    bool canAttack = true;
    bool isDead;

    Transform target;
    EnemySpawner spawner;
    SpriteRenderer sr;
    Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
            gameObject.layer = enemyLayer;
    }

    void Start()
    {
        hp = maxHp;
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
            target = playerGo.transform;
        else
            Debug.LogWarning("[Enemy] Player tagged object not found.");
    }

    void Update()
    {
        if (isDead) return;
        if (target == null)
        {
            FindPlayer();
            if (target == null) return;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        var player = target.GetComponent<PlayerController2D>();
        if (player != null && player.CurrentHp <= 0)
        {
            animator?.SetBool("isMoving", false);
            return;
        }

        if (dist <= attackRange)
        {
            animator?.SetBool("isMoving", false);

            if (canAttack)
                StartCoroutine(DoAttack(player));
            return;
        }

        animator?.SetBool("isMoving", true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (sr != null)
        {
            if (target.position.x < transform.position.x)
                sr.flipX = true;
            else
                sr.flipX = false;
        }
    }

    IEnumerator DoAttack(PlayerController2D cachedPlayer)
    {
        canAttack = false;

        animator?.SetTrigger("attack");

        yield return new WaitForSeconds(0.15f);

        var player = cachedPlayer != null ? cachedPlayer : target.GetComponent<PlayerController2D>();
        if (player != null)
            player.TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        hp -= Mathf.Max(1, dmg);

        animator?.SetTrigger("hit");

        if (sr != null)
            StartCoroutine(Flash());

        Debug.Log($"[Enemy Hit] {gameObject.name} : -{dmg} (HP: {hp})");

        if (hp <= 0)
            Die();
    }

    IEnumerator Flash()
    {
        if (sr == null) yield break;

        Color orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.07f);
        sr.color = orig;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        animator?.SetTrigger("die");
        animator?.SetBool("isMoving", false);
        canAttack = false;

        if (spawner != null)
            spawner.OnEnemyKilled(gameObject);

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

