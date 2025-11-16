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

    Transform target;
    EnemySpawner spawner;
    SpriteRenderer sr;
    Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // ⭐ Animator 연결
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
    }

    void Start()
    {
        hp = maxHp;

        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
            target = playerGo.transform;
        else
            Debug.LogWarning("[Enemy] Player tagged object not found.");
    }

    void Update()
    {
        if (hp <= 0 || target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);

        // 공격 범위 안 → 공격
        if (dist <= attackRange)
        {
            animator.SetBool("isMoving", false);

            if (canAttack)
                StartCoroutine(DoAttack());
            return;
        }

        // 이동
        animator.SetBool("isMoving", true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // 방향 전환
        if (target.position.x < transform.position.x)
            sr.flipX = true;
        else
            sr.flipX = false;
    }

    // -----------------------------------------
    // 공격
    // -----------------------------------------
    IEnumerator DoAttack()
    {
        canAttack = false;

        // ⭐ 공격 애니메이션 트리거
        animator.SetTrigger("attack");

        yield return new WaitForSeconds(0.15f); // 타격 타이밍(애니에 따라 조절)

        // 데미지 적용
        var player = target.GetComponent<PlayerController2D>();
        if (player != null)
            player.TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    // -----------------------------------------
    // 피격
    // -----------------------------------------
    public void TakeDamage(int dmg)
    {
        hp -= Mathf.Max(1, dmg);

        // ⭐ Hit 애니메이션 재생
        animator.SetTrigger("hit");

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

    // -----------------------------------------
    // 사망
    // -----------------------------------------
    void Die()
    {
        animator.SetTrigger("die");  // ⭐ Death 애니 재생
        animator.SetBool("isMoving", false);
        canAttack = false;

        // EnemySpawner 카운트 처리
        if (spawner != null)
            spawner.OnEnemyKilled(gameObject);

        // 애니메이션 재생 끝나고 삭제
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.5f); // death 애니 길이에 맞춰 조절
        Destroy(gameObject);
    }
}


