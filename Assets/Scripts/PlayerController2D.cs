using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f; // 작은 입력 무시

    [Header("컴포넌트 참조")]
    [SerializeField] private Rigidbody2D rb = null;
    [SerializeField] private Animator animator = null;
    [SerializeField] private SpriteRenderer spriteRenderer = null;

    private Vector2 moveInput = Vector2.zero;
    private bool isAttacking = false;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 공격 입력 (좌클릭)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            animator.SetTrigger("attack");
            isAttacking = true;
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

    // 공격 애니메이션 끝날 때 Animation Event에서 호출
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
}
