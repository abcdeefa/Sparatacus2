using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public int maxHp = 500;
    public float moveSpeed = 2f;
    public int meleeDamage = 20;
    public int skill1Damage = 15;
    public int skill2Damage = 10;
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
    Transform target;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        hp = maxHp;
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) target = playerGo.transform;
    }

    void Update()
    {
        if (hp <= 0 || target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist <= meleeRange)
        {
            if (canAttack)
                StartCoroutine(DoAttack(AttackType.Melee));
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        sr.flipX = target.position.x < transform.position.x;
    }

    enum AttackType { Melee, Skill1, Skill2, Skill3 }

    IEnumerator DoAttack(AttackType type)
    {
        canAttack = false;
        ApplyDamage(type);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void ApplyDamage(AttackType type)
    {
        float range = meleeRange;
        int dmg = meleeDamage;

        switch (type)
        {
            case AttackType.Melee: range = meleeRange; dmg = meleeDamage; break;
            case AttackType.Skill1: range = skill1Range; dmg = skill1Damage; break;
            case AttackType.Skill2: range = skill2Range; dmg = skill2Damage; break;
            case AttackType.Skill3: range = skill3Range; dmg = skill3Damage; break;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, range, playerLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerController2D>()?.TakeDamage(dmg);
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= Mathf.Max(1, dmg);
        if (hp <= 0) Die();
    }

    void Die()
    {
        canAttack = false;
        Destroy(gameObject, 0.5f);
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
