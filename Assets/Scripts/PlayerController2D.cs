using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    public Transform attackOrigin;
    public LayerMask enemyLayer;

    public float lmbRange = 1.1f;
    public int lmbDamage = 20;

    public float rmbRange = 1.6f;
    public int rmbDamage = 35;

    public float hitDelay = 0.08f;
    public float attackCooldown = 0.25f;

    public int maxHp = 100;
    public float hitInvincibleTime = 0.3f;

    public bool showAttackRange = true;
    public Color attackColor = new Color(1f, 0f, 0f, 0.5f);

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
            if (spriteRenderer != null && isAttacking)
                spriteRenderer.flipX = !attackFacingRight;
        }

        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking && !isHit)
        {
            attackFacingRight = !spriteRenderer.flipX;
            StartCoroutine(DoAttack(lmbRange, lmbDamage, "attack"));
        }
        else if (Input.GetMouseButtonDown(1) && canAttack && !isAttacking && !isHit)
        {
            attackFacingRight = !spriteRenderer.flipX;
            StartCoroutine(DoAttack(rmbRange, rmbDamage, "attackHeavy"));
        }
    }

    void FixedUpdate()
    {
        if (hp <= 0 || rb == null) return;
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
        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;
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

    void OnDrawGizmos()
    {
        if (!showAttackRange || attackOrigin == null) return;

        Gizmos.color = attackColor;
        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;

        Vector2 lmbCenter = (Vector2)attackOrigin.position + dir * (lmbRange * 0.6f);
        Gizmos.DrawWireSphere(lmbCenter, lmbRange);

        Vector2 rmbCenter = (Vector2)attackOrigin.position + dir * (rmbRange * 0.6f);
        Gizmos.DrawWireSphere(rmbCenter, rmbRange);
    }

    void OnRenderObject()
    {
        if (!showAttackRange || attackOrigin == null) return;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        GL.Begin(GL.LINE_STRIP);
        GL.Color(attackColor);
        int segments = 30;

        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;
        Vector2 lmbCenter = (Vector2)attackOrigin.position + dir * (lmbRange * 0.6f);
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * lmbRange + lmbCenter.x;
            float y = Mathf.Sin(angle) * lmbRange + lmbCenter.y;
            GL.Vertex3(x, y, 0);
        }

        Vector2 rmbCenter = (Vector2)attackOrigin.position + dir * (rmbRange * 0.6f);
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * rmbRange + rmbCenter.x;
            float y = Mathf.Sin(angle) * rmbRange + rmbCenter.y;
            GL.Vertex3(x, y, 0);
        }

        GL.End();
        GL.PopMatrix();
    }
}
