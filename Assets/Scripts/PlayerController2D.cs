using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float deadZone = 0.01f;

    public int CurrentHp => hp;

    [Header("Components")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Combat")]
    public Transform attackOrigin;
    public LayerMask enemyLayer;
    public float lmbRange = 1.1f;
    public int lmbDamage = 20;
    public float rmbRange = 1.6f;
    public int rmbDamage = 35;
    public float hitDelay = 0.08f;
    public float attackCooldown = 0.25f;

    [Header("Survivability")]
    public int maxHp = 100;
    public float hitInvincibleTime = 0.3f;

    [Header("Dash (Q)")]
    public float dashDistance = 2f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 5f;

    [Header("Skill1 (E)")]
    public float skill1Cooldown = 6f;
    public float skill1RangeSword = 1.8f;
    public float skill1RangeSpear = 2.2f;
    public int skill1DamageSword = 35;
    public int skill1DamageSpear = 40;
    public float skill1AngleSword = 120f;
    public float skill1AngleSpear = 60f;

    [Header("Skill2 (R)")]
    public float skill2Cooldown = 15f;
    public int skill2DamageSword = 80;
    public int skill2DamageSpear = 70;
    public float skill2CastDelay = 0.3f;
    public float skill2DashDistance = 3.5f;

    [Header("Guard (F)")]
    public float guardSpeedMultiplier = 0.4f;
    public float guardDamageMultiplier = 0.4f;

    [Header("Debug/Visualize")]
    public bool showAttackRange = true;
    public Color attackColor = new Color(1f, 0f, 0f, 0.5f);

    public System.Action onDeath;
    public System.Action<int, int> onHealthChanged;

    int hp;
    Vector2 moveInput;
    bool isAttacking;
    bool canAttack = true;
    bool isHit;
    bool isDashing;
    bool isGuarding;
    bool isInvincible;
    bool isDead;
    bool attackFacingRight = true;
    float lastDashTime = -999f;
    float lastSkill1Time = -999f;
    float lastSkill2Time = -999f;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        EnsureRefs();
    }

    void Start()
    {
        hp = maxHp;
        ApplyClassTuning();
        EnsureAttackOrigin();
        EnsureEnemyLayer();
        onHealthChanged?.Invoke(hp, maxHp);
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 1;
    }

    void EnsureRefs()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void EnsureAttackOrigin()
    {
        if (attackOrigin != null) return;
        var child = transform.Find("attackorigin") ?? transform.Find("attackOrigin");
        attackOrigin = child != null ? child : transform;
    }

    void EnsureEnemyLayer()
    {
        if (enemyLayer == 0)
        {
            int mask = LayerMask.GetMask("Enemy");
            enemyLayer = mask == 0 ? Physics2D.AllLayers : mask;
        }
    }

    void ApplyClassTuning()
    {
        if (PlayerChoice.Selected == CharacterClass.Spear)
        {
            moveSpeed = 3.3f;
            lmbRange = 1.5f;
            lmbDamage = 18;
            rmbRange = 1.8f;
            rmbDamage = 40;
            attackCooldown = 0.28f;
            if (animator != null) animator.SetFloat("attackSpeedMul", 0.95f);
        }
        else
        {
            moveSpeed = 3.7f;
            lmbRange = 1.1f;
            lmbDamage = 20;
            rmbRange = 1.4f;
            rmbDamage = 35;
            attackCooldown = 0.22f;
            if (animator != null) animator.SetFloat("attackSpeedMul", 1.08f);
        }
    }

    void Update()
    {
        if (isDead) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool allowMovement = !isAttacking && !isHit && !isDashing;
        moveInput = allowMovement ? new Vector2(h, v) : Vector2.zero;

        bool isMoving = moveInput.sqrMagnitude > deadZone;
        if (h != 0f && spriteRenderer != null && allowMovement)
        {
            spriteRenderer.flipX = h < 0f;
            attackFacingRight = !spriteRenderer.flipX;
        }

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        HandleGuard();
        HandleCombatInput();
    }

    void HandleGuard()
    {
        bool guardKey = Input.GetKey(KeyCode.F);
        isGuarding = guardKey && !isDashing && !isHit && !isDead && !isAttacking;
        if (animator != null)
            animator.SetBool("isGuarding", isGuarding);
    }

    void HandleCombatInput()
    {
        if (isHit || isDashing || isDead) return;

        if (Input.GetMouseButtonDown(0) && canAttack && !isAttacking && !isGuarding)
        {
            attackFacingRight = spriteRenderer == null || !spriteRenderer.flipX;
            StartCoroutine(DoAttack(lmbRange, lmbDamage, "attack", 45f));
        }
        else if (Input.GetMouseButtonDown(1) && canAttack && !isAttacking && !isGuarding)
        {
            attackFacingRight = spriteRenderer == null || !spriteRenderer.flipX;
            StartCoroutine(DoAttack(rmbRange, rmbDamage, "attackHeavy", 55f));
        }

        if (Input.GetKeyDown(KeyCode.Q))
            TryDash();

        if (Input.GetKeyDown(KeyCode.E))
            TrySkill1();

        if (Input.GetKeyDown(KeyCode.R))
            TrySkill2();
    }

    void FixedUpdate()
    {
        if (isDead || rb == null || isDashing) return;
        Vector2 direction = moveInput.normalized;
        float speed = moveSpeed * (isGuarding ? guardSpeedMultiplier : 1f);
        Vector2 target = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(target);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    void TryDash()
    {
        if (Time.time < lastDashTime + dashCooldown) return;
        if (isAttacking || isHit || isGuarding || isDashing) return;
        attackFacingRight = spriteRenderer == null || !spriteRenderer.flipX;
        StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        lastDashTime = Time.time;
        isDashing = true;
        isInvincible = true;
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("dash");

        Vector2 start = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;
        Vector2 end = start + dir * dashDistance;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(start, end, elapsed / dashDuration);
            if (rb != null) rb.MovePosition(pos);
            yield return null;
        }

        isDashing = false;
        isInvincible = false;
        canAttack = true;
    }

    void TrySkill1()
    {
        if (Time.time < lastSkill1Time + skill1Cooldown) return;
        if (isAttacking || isHit || isDashing || isGuarding) return;
        attackFacingRight = spriteRenderer == null || !spriteRenderer.flipX;
        StartCoroutine(Skill1Coroutine());
    }

    IEnumerator Skill1Coroutine()
    {
        lastSkill1Time = Time.time;
        isAttacking = true;
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("skill1");

        yield return new WaitForSeconds(hitDelay);

        bool spear = PlayerChoice.Selected == CharacterClass.Spear;
        float range = spear ? skill1RangeSpear : skill1RangeSword;
        int dmg = spear ? skill1DamageSpear : skill1DamageSword;
        float angle = spear ? skill1AngleSpear : skill1AngleSword;
        PerformArcAttack(range, angle, dmg);

        yield return new WaitForSeconds(skill1Cooldown * 0.1f);
        isAttacking = false;
        canAttack = true;
    }

    void TrySkill2()
    {
        if (Time.time < lastSkill2Time + skill2Cooldown) return;
        if (isAttacking || isHit || isDashing) return;
        attackFacingRight = spriteRenderer == null || !spriteRenderer.flipX;
        StartCoroutine(Skill2Coroutine());
    }

    IEnumerator Skill2Coroutine()
    {
        lastSkill2Time = Time.time;
        isAttacking = true;
        canAttack = false;
        isInvincible = true;

        if (animator != null)
            animator.SetTrigger("skill2");

        yield return new WaitForSeconds(skill2CastDelay);

        bool spear = PlayerChoice.Selected == CharacterClass.Spear;
        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position;

        if (spear)
        {
            Vector2 startPos = rb != null ? rb.position : (Vector2)transform.position;
            Vector2 endPos = startPos + dir * skill2DashDistance;
            float elapsed = 0f;
            float duration = Mathf.Max(0.18f, dashDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector2 pos = Vector2.Lerp(startPos, endPos, elapsed / duration);
                if (rb != null) rb.MovePosition(pos);
                yield return null;
            }

            Vector2 center = (startPos + endPos) * 0.5f;
            Vector2 size = new Vector2(skill2DashDistance + 0.8f, 1.2f);
            DealBoxDamage(center, size, dir, skill2DamageSpear);
        }
        else
        {
            Vector2 size = new Vector2(3.2f, 1.4f);
            Vector2 center = origin + dir * (size.x * 0.5f);
            DealBoxDamage(center, size, dir, skill2DamageSword);
        }

        yield return new WaitForSeconds(0.1f);
        isInvincible = false;
        isAttacking = false;
        canAttack = true;
    }

    IEnumerator DoAttack(float range, int damage, string animTrigger, float angleLimit)
    {
        isAttacking = true;
        canAttack = false;

        if (animator != null && !string.IsNullOrEmpty(animTrigger))
            animator.SetTrigger(animTrigger);

        yield return new WaitForSeconds(hitDelay);
        PerformArcAttack(range, angleLimit * 2f, damage);
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    void PerformArcAttack(float range, float arcAngle, int damage)
    {
        Vector2 origin = attackOrigin ? (Vector2)attackOrigin.position : (Vector2)transform.position;
        Vector2 dir = attackFacingRight ? Vector2.right : Vector2.left;
        Vector2 center = origin + dir * (range * 0.6f);

        var hits = Physics2D.OverlapCircleAll(center, range, enemyLayer);
        float halfAngle = arcAngle * 0.5f;

        foreach (var h in hits)
        {
            if (h == null) continue;
            Vector2 toTarget = ((Vector2)h.transform.position - origin).normalized;
            float angle = Vector2.Angle(dir, toTarget);
            if (angle > halfAngle) continue;

            h.GetComponent<EnemyController>()?.TakeDamage(damage);
            h.GetComponent<BossController>()?.TakeDamage(damage);
        }
    }

    void DealBoxDamage(Vector2 center, Vector2 size, Vector2 facing, int damage)
    {
        float angle = facing.x >= 0 ? 0f : 180f;
        var hits = Physics2D.OverlapBoxAll(center, size, angle, enemyLayer);
        foreach (var h in hits)
        {
            h.GetComponent<EnemyController>()?.TakeDamage(damage);
            h.GetComponent<BossController>()?.TakeDamage(damage);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isHit || isInvincible || isDashing || isDead) return;

        int finalDmg = isGuarding ? Mathf.CeilToInt(dmg * guardDamageMultiplier) : dmg;
        hp -= Mathf.Max(1, finalDmg);
        onHealthChanged?.Invoke(hp, maxHp);
        isHit = true;

        if (animator != null)
            animator.SetTrigger("hit");

        if (hp <= 0)
        {
            hp = 0;
            Die();
            return;
        }

        StartCoroutine(HitInvincible());
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        moveInput = Vector2.zero;
        canAttack = false;
        isAttacking = false;

        if (animator != null)
            animator.SetTrigger("die");

        onDeath?.Invoke();
    }

    IEnumerator HitInvincible()
    {
        float elapsed = 0f;
        Color original = spriteRenderer != null ? spriteRenderer.color : Color.white;
        isInvincible = true;

        while (elapsed < hitInvincibleTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
            yield return new WaitForSeconds(0.1f);

            if (spriteRenderer != null)
                spriteRenderer.color = original;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        isInvincible = false;
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

        var mat = new Material(Shader.Find("Sprites/Default"));
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
