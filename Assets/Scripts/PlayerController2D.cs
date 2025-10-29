using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f; // ���� �Է� ����

    [Header("������Ʈ ����")]
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
        // ���� �Է� (��Ŭ��)
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            animator.SetTrigger("attack");
            isAttacking = true;
        }

        // �̵� �Է� (���� �� �̵� ����)
        if (!isAttacking)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical);

            // DeadZone ����
            bool isMoving = moveInput.sqrMagnitude > deadZone;

            // �¿� flip ó��
            if (horizontal != 0f && spriteRenderer != null)
                spriteRenderer.flipX = horizontal < 0f;

            // Animator�� �̵� ���� ����
            if (animator != null)
                animator.SetBool("isMoving", isMoving);
        }
        else
        {
            moveInput = Vector2.zero; // ���� �� �̵� �Ұ�
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // ���� �̵�
        Vector2 targetPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    // ���� �ִϸ��̼� ���� �� Animation Event���� ȣ��
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
}
