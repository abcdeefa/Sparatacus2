using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public int maxHp = 500;
    public float moveSpeed = 2f;
    public int meleeDamage = 20;
    public int skill1Damage = 15;
    public int skill2Damage = 20;
    public int skill3Damage = 30;

    public Transform attackPoint;
    public float meleeRange = 1f;
    public float skill1Range = 2f;
    public float skill2Range = 3f;
    public float skill3Range = 5f;
    public LayerMask playerLayer;
    public float attackCooldown = 1f;

    public Color meleeColor = Color.red;
    public Color skillColor = Color.yellow;

    int hp;
    bool canAttack = true;
    bool isAttacking;
    bool isDead;

    Transform target;
    SpriteRenderer sr;
    Animator animator;
    EnemySpawner spawner;

    enum AttackType { Melee, Skill1, Skill2, Skill3 }

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
        if (playerLayer == 0)
        {
            int mask = LayerMask.GetMask("Player");
            playerLayer = mask == 0 ? Physics2D.AllLayers : mask;
        }
    }

    void FindPlayer()
    {
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) target = playerGo.transform;
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
        FaceTarget();

        if (isAttacking) return;

        float hpRate = (float)hp / maxHp;
        if (hpRate > 0.7f)
        {
            MoveTowardsTarget(dist, meleeRange);
            if (dist <= meleeRange && canAttack)
                StartCoroutine(DoAttack(AttackType.Melee));
        }
        else if (hpRate > 0.4f)
        {
            MoveTowardsTarget(dist, skill1Range);
            if (canAttack)
            {
                if (dist <= skill1Range)
                    StartCoroutine(DoAttack(Random.value < 0.5f ? AttackType.Skill1 : AttackType.Melee));
                else if (dist <= meleeRange)
                    StartCoroutine(DoAttack(AttackType.Melee));
            }
        }
        else
        {
            MoveTowardsTarget(dist, skill2Range);
            if (canAttack)
            {
                if (dist <= skill2Range)
                    StartCoroutine(DoAttack(Random.value < 0.4f ? AttackType.Skill2 : AttackType.Melee));
                else if (dist <= skill3Range)
                    StartCoroutine(DoAttack(AttackType.Skill3));
            }
        }
    }

    void MoveTowardsTarget(float dist, float stopRange)
    {
        if (dist <= stopRange)
        {
            animator?.SetBool("isMoving", false);
            return;
        }

        animator?.SetBool("isMoving", true);
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);
    }

    void FaceTarget()
    {
        if (target == null || sr == null) return;
        sr.flipX = target.position.x < transform.position.x;
    }

    IEnumerator DoAttack(AttackType type)
    {
        canAttack = false;
        isAttacking = true;

        TriggerAnimation(type);
        yield return new WaitForSeconds(GetWindup(type));

        ApplyDamage(type);
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        canAttack = true;
    }

    float GetWindup(AttackType type)
    {
        switch (type)
        {
            case AttackType.Skill1: return 0.35f;
            case AttackType.Skill2: return 0.25f;
            case AttackType.Skill3: return 0.3f;
            default: return 0.2f;
        }
    }

    void TriggerAnimation(AttackType type)
    {
        if (animator == null) return;
        switch (type)
        {
            case AttackType.Skill1: animator.SetTrigger("skill1"); break;
            case AttackType.Skill2: animator.SetTrigger("skill2"); break;
            case AttackType.Skill3: animator.SetTrigger("skill3"); break;
            default: animator.SetTrigger("attack"); break;
        }
    }

    void ApplyDamage(AttackType type)
    {
        float range = meleeRange;
        int dmg = meleeDamage;

        switch (type)
        {
            case AttackType.Skill1: range = skill1Range; dmg = skill1Damage; break;
            case AttackType.Skill2: range = skill2Range; dmg = skill2Damage; break;
            case AttackType.Skill3: range = skill3Range; dmg = skill3Damage; break;
        }

        Vector2 pos = attackPoint ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, range, playerLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerController2D>()?.TakeDamage(dmg);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        hp -= Mathf.Max(1, dmg);
        animator?.SetTrigger("hit");
        if (sr != null) StartCoroutine(Flash());
        if (hp <= 0) Die();
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
        canAttack = false;
        animator?.SetTrigger("die");
        if (spawner != null)
            spawner.OnEnemyKilled(gameObject);
        Destroy(gameObject, 0.6f);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = meleeColor;
        Gizmos.DrawWireSphere(attackPoint.position, meleeRange);

        Gizmos.color = skillColor;
        Gizmos.DrawWireSphere(attackPoint.position, skill1Range);
        Gizmos.DrawWireSphere(attackPoint.position, skill2Range);
        Gizmos.DrawWireSphere(attackPoint.position, skill3Range);
    }
}
