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

    Vector2 moveInput;
    bool isAttacking;
    bool canAttack = true;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canAttack && !isAttacking)
        {
            StartCoroutine(DoAttack(rmbRange, rmbDamage, "attackHeavy"));
            return;
        }

        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking)
        {
            StartCoroutine(DoAttack(lmbRange, lmbDamage, "attack"));
            return;
        }

        if (!isAttacking)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            moveInput = new Vector2(h, v);

            bool isMoving = moveInput.sqrMagnitude > deadZone;

            if (h != 0f && spriteRenderer != null)
                spriteRenderer.flipX = h < 0f;

            if (animator != null)
                animator.SetBool("isMoving", isMoving);
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
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
            if (ec != null)
            {
                ec.TakeDamage(damage);
                continue;
            }

            var bc = h.GetComponent<BossController>();
            if (bc != null)
            {
                bc.TakeDamage(damage);
                continue;
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    void OnDrawGizmosSelected()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
        Vector2 dir = spriteRenderer && spriteRenderer.flipX ? Vector2.left : Vector2.right;

        DrawFanGizmo(origin, dir, lmbRange, 90f, new Color(1, 0, 0, 0.6f));
        DrawFanGizmo(origin, dir, rmbRange, 90f, new Color(1, 0.5f, 0, 0.4f));
    }

    void DrawFanGizmo(Vector3 origin, Vector2 dir, float range, float angle, Color color)
    {
        Gizmos.color = color;

        Gizmos.DrawLine(origin, origin + (Vector3)(dir * range));

        Quaternion leftRot = Quaternion.Euler(0, 0, angle * 0.5f);
        Quaternion rightRot = Quaternion.Euler(0, 0, -angle * 0.5f);

        Vector2 leftDir = leftRot * dir;
        Vector2 rightDir = rightRot * dir;

        Gizmos.DrawLine(origin, origin + (Vector3)(leftDir * range));
        Gizmos.DrawLine(origin, origin + (Vector3)(rightDir * range));

        int segments = 20;
        float step = angle / segments;

        Vector2 prev = origin + (Vector3)(rightDir * range);
        for (int i = 1; i <= segments; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, -angle * 0.5f + step * i);
            Vector2 nextDir = Quaternion.Euler(0, 0, -angle * 0.5f + step * i) * dir;
            Vector2 next = origin + (Vector3)(nextDir * range);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
