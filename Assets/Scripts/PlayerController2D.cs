using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Attack")]
    public Transform attackOrigin;
    public LayerMask enemyLayer;

    public float lmbRange = 1.1f;
    public int lmbDamage = 20;

    public float rmbRange = 1.6f;
    public int rmbDamage = 35;

    public float hitDelay = 0.08f;
    public float attackCooldown = 0.25f;

    [Header("Player Stats")]
    public int maxHp = 100;
    public float hitInvincibleTime = 0.3f; // 피격 후 무적 시간

    int hp;
    Vector2 moveInput;
    bool isAttacking;
    bool canAttack = true;
    bool isHit = false;
    bool attackFacingRight = true;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        hp = maxHp;
    }

    void Update()
    {
        if (hp <= 0) return;

        float h = Input.GetAxisRaw("Horizontal");

        // 이동 처리
        if (!isAttacking && !isHit)
        {
            moveInput = new Vector2(h, Input.GetAxisRaw("Vertical"));
            bool isMoving = moveInput.sqrMagnitude > deadZone;

            if (h != 0f && spriteRenderer != null)
                spriteRenderer.flipX = h < 0f;

            if (animator != null)
                animator.SetBool("isMoving", isMoving);
        }
        else
        {
            moveInput = Vector2.zero;
            // 공격 중 방향 고정
            if (spriteRenderer != null && isAttacking)
                spriteRenderer.flipX = !attackFacingRight;
        }

        // 공격 입력
        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking && !isHit)
        {
            attackFacingRight = h >= 0;
            StartCoroutine(DoAttack(lmbRange, lmbDamage, "attack"));
        }
        else if (Input.GetMouseButtonDown(1) && canAttack && !isAttacking && !isHit)
        {
            attackFacingRight = h >= 0;
            StartCoroutine(DoAttack(rmbRange, rmbDamage, "attackHeavy"));
        }
    }

    void FixedUpdate()
    {
        if (hp <= 0) return;

        if (rb == null) return;

        Vector2 target = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(target);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    IEnumerator DoAttack(float range, int damage, string animTrigger)
    {
        isAttacking = true;
        canAttack = false;

        if (animator != null && !string.IsNullOrEmpty(animTrigger))
            animator.SetTrigger(animTrigger);

        Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
        Vector2 dir = spriteRenderer && spriteRenderer.flipX ? Vector2.left : Vector2.right;

        Vector2 center = (Vector2)origin + dir * (range * 0.6f);

        yield return new WaitForSeconds(hitDelay);

        var hits = Physics2D.OverlapCircleAll(center, range, enemyLayer);
        foreach (var h in hits)
        {
            Vector2 toTarget = (h.transform.position - origin).normalized;
            float angle = Vector2.Angle(dir, toTarget);
            if (angle > 45f) continue;

            var ec = h.GetComponent<EnemyController>();
            if (ec != null) ec.TakeDamage(damage);

            var bc = h.GetComponent<BossController>();
            if (bc != null) bc.TakeDamage(damage);
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    // 플레이어 피격
    public void TakeDamage(int dmg)
    {
        if (isHit || hp <= 0) return;

        hp -= dmg;
        isHit = true;

        if (animator != null)
            animator.SetTrigger("hit");

        if (hp <= 0)
        {
            if (animator != null)
                animator.SetTrigger("die");

            moveInput = Vector2.zero;
            canAttack = false;
            return;
        }

        StartCoroutine(HitInvincible());
    }

    // Hit 무적 코루틴
    IEnumerator HitInvincible()
    {
        float elapsed = 0f;
        Color original = spriteRenderer.color;

        while (elapsed < hitInvincibleTime)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        isHit = false;
    }
}
