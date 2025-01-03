using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GameObject healthBarPrefab;
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float attackSpeed = 1f;
    public float attackRange = 1f;
    public float moveSpeed = 5f;
    public Animator animator;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public string team; // Team identifier

    private int currentHealth;
    private bool isAttacking = false;
    private GameObject healthBar;
    private GameObject target;
    private Rigidbody2D rb;
    private float movementDirection = 0f;
    private bool isDead = false;
    private Collider2D myCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        healthBar = Instantiate(healthBarPrefab, transform.position, Quaternion.identity, transform);
        healthBar.transform.localPosition = new Vector3(0, 5.4f, 0);

        HealthBar healthBarScript = healthBar.GetComponentInChildren<HealthBar>();
        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHealth(maxHealth);
        }
        myCollider = GetComponent<Collider2D>();
    }

    private void CheckForTargetsInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        float closestDistance = Mathf.Infinity;
        GameObject closestTarget = null;

        foreach (var hit in hits)
        {
            Unit hitUnit = hit.GetComponent<Unit>();

            if (hit.gameObject != gameObject && hitUnit != null && hitUnit.team != team)
            {
                Vector2 closestPoint = hit.ClosestPoint(transform.position);
                float distance = Vector2.Distance(transform.position, closestPoint);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.gameObject;
                }
            }
        }

        target = closestTarget;

        if (target != null && !isAttacking)
        {
            StartCoroutine(Attack());
        }
        else if (target == null && isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void FixedUpdate()
    {
        CheckForTargetsInRange();

        if (rb.bodyType != RigidbodyType2D.Static)
        {
            if (movementDirection != 0 && !isAttacking && !isDead)
            {
                rb.linearVelocity = new Vector2(movementDirection * moveSpeed, rb.linearVelocity.y);
                if (animator != null)
                {
                    animator.SetFloat("Speed", Mathf.Abs(moveSpeed));
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void SetDirection(float dir)
    {
        movementDirection = dir;
        UpdateDirection(dir);
    }

    void UpdateDirection(float direction)
    {
        if (direction != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        while (target != null)
        {
            if (Vector2.Distance(transform.position, target.GetComponent<Collider2D>().ClosestPoint(transform.position)) <= attackRange)
            {
                _TriggerAttack();

                if (projectilePrefab == null || projectileSpawnPoint == null)
                {
                    Unit targetUnit = target.GetComponent<Unit>();
                    if (targetUnit != null)
                    {
                        targetUnit.TakeDamage(attackDamage);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                    Projectile projectileScript = projectile.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        Vector2 direction = (target.transform.position - transform.position).normalized;
                        float distance = Vector2.Distance(transform.position, target.transform.position);
                        projectileScript.Initialize(direction, distance);
                        projectile.tag = gameObject.tag;
                    }
                }

                yield return new WaitForSeconds(GetAnimationLength("Attack"));
                yield return new WaitForSeconds(attackSpeed);
            }
            else
            {
                break;
            }

        }
        isAttacking = false;
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }

    private float GetAnimationLength(string animationName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0.5f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 0.5f;
    }

    private void _TriggerAttack()
    {
        if (animator != null && target != null && target.GetComponent<Unit>() != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
            Destroy(gameObject, GetAnimationLength("Die"));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
        {
            HealthBar healthBarScript = healthBar.GetComponentInChildren<HealthBar>();
            healthBarScript.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}