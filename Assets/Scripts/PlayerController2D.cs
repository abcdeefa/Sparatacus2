using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f; // 작은 입력 무시

    [Header("컴포넌트 참조")]
    [SerializeField] private Rigidbody2D rb = null;
    [SerializeField] private Animator animator = null;
    [SerializeField] private SpriteRenderer spriteRenderer = null;

    [Header("공격 설정")]
    public Transform attackOrigin;          // 플레이어 앞쪽에 빈 오브젝트 하나 만들어 연결(없으면 transform 사용)
    public LayerMask enemyLayer;            // Enemy 레이어 체크
    public float lmbRange = 1.1f;           // 좌클릭 범위
    public int lmbDamage = 20;            // 좌클릭 데미지
    public float rmbRange = 1.6f;           // 우클릭 범위(넓음)
    public int rmbDamage = 35;            // 우클릭 데미지(강함)
    public float hitDelay = 0.08f;          // 클릭 후 실제 히트 타이밍(애니와 맞추기)
    public float attackCooldown = 0.25f;    // 연타 쿨다운

    private Vector2 moveInput = Vector2.zero;
    private bool isAttacking = false;
    private bool canAttack = true;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // --- 우클릭(강공격) ---
        if (Input.GetMouseButtonDown(1) && canAttack && !isAttacking)
        {
            StartCoroutine(DoAttack(rmbRange, rmbDamage, "attackHeavy"));
            return;
        }

        // --- 좌클릭(기본공격) ---
        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking)
        {
            StartCoroutine(DoAttack(lmbRange, lmbDamage, "attack"));
            return;
        }

        // 이동 입력 (공격 중 이동 제한)
        if (!isAttacking)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical);

            // DeadZone 적용
            bool isMoving = moveInput.sqrMagnitude > deadZone;

            // 좌우 flip 처리
            if (horizontal != 0f && spriteRenderer != null)
                spriteRenderer.flipX = horizontal < 0f;

            // Animator에 이동 상태 전달
            if (animator != null)
                animator.SetBool("isMoving", isMoving);
        }
        else
        {
            moveInput = Vector2.zero; // 공격 중 이동 불가
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // 물리 이동
        Vector2 targetPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    // 애니메이션 이벤트용(필요시 유지)
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    // ===== 공격 코어 =====
    IEnumerator DoAttack(float range, int damage, string animTriggerName)
    {
        isAttacking = true;
        canAttack = false;

        // 애니 트리거(없어도 무시됨)
        if (animator != null && !string.IsNullOrEmpty(animTriggerName))
            animator.SetTrigger(animTriggerName);

        // 마우스 방향으로 약간 앞쪽에서 판정
        Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
        Vector3 mouseWorld = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : origin + Vector3.right;
        mouseWorld.z = origin.z;
        Vector2 dir = (mouseWorld - origin).normalized;
        Vector2 center = (Vector2)origin + dir * (range * 0.6f);

        // 애니 히트 타이밍 맞추기
        yield return new WaitForSeconds(hitDelay);

        // 범위 내 적 타격
        var hits = Physics2D.OverlapCircleAll(center, range, enemyLayer);
        foreach (var h in hits)
        {
            // EnemyController / BossController 둘 다 지원
            var ec = h.GetComponent<EnemyController>();
            if (ec != null) { ec.TakeDamage(damage); continue; }

            var bc = h.GetComponent<BossController>();
            if (bc != null) { bc.TakeDamage(damage); continue; }
        }

        // 짧게 딜레이 후 공격 가능
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    // 씬에서 공격 범위 확인(선택)
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
        Vector3 mouseWorld = Camera.main ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : origin + Vector3.right;
        mouseWorld.z = origin.z;
        Vector2 dir = (mouseWorld - origin).normalized;

        Gizmos.color = new Color(1, 0, 0, 0.6f);
        Gizmos.DrawWireSphere(origin + (Vector3)(dir * (lmbRange * 0.6f)), lmbRange);
        Gizmos.color = new Color(1, 0.5f, 0, 0.4f);
        Gizmos.DrawWireSphere(origin + (Vector3)(dir * (rmbRange * 0.6f)), rmbRange);
    }
}
