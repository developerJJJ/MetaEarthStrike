using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GameObject healthBarPrefab; // Reference to the Health Bar prefab
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float attackSpeed = 1f; // Time in seconds between attacks
    public float attackRange = 1f; // Range within which the unit attacks
    public float moveSpeed = 5f; // Speed of movement
    public Animator animator; // Reference to Animator
    public GameObject projectilePrefab; // Reference to the Projectile prefab
    public Transform projectileSpawnPoint; // Point from which the projectile is spawned

    private int currentHealth;
    private bool isAttacking = false; // Tracks if the unit is currently attacking
    private GameObject healthBar;
    private GameObject target; // Current target enemy
    private Rigidbody2D rb; // Reference to Rigidbody2D
    private float movementDirection = 0f; // Movement direction (-1 for left, 1 for right)
    private bool isDead = false; // Tracks if the unit is dead

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Initialize Rigidbody2D
        animator = GetComponent<Animator>(); // Initialize Animator
        if (animator == null)
        {
            Debug.LogError("Animator component is missing from the unit.");
        }

        currentHealth = maxHealth;

        // Instantiate the health bar and set it as a child of the unit
        healthBar = Instantiate(healthBarPrefab, transform.position, Quaternion.identity, transform);
        healthBar.transform.localPosition = new Vector3(0, 5.4f, 0);

        // Set max health in the HealthBar script
        HealthBar healthBarScript = healthBar.GetComponentInChildren<HealthBar>();
        if (healthBarScript != null)
        {
            healthBarScript.SetMaxHealth(maxHealth);
        }
    }
    private void CheckForTargetsInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hit in hits)
        {
            // Check if the target is an enemy
            if ((gameObject.CompareTag("PlayerUnit") && hit.CompareTag("EnemyUnit")) ||
                (gameObject.CompareTag("EnemyUnit") && hit.CompareTag("PlayerUnit")))
            {
                target = hit.gameObject; // Set the target
                if (!isAttacking)
                {
                    StartCoroutine(Attack()); // Start attacking the target
                }
                break;
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

        // Move the unit using Rigidbody2D (physics-based movement)
        if (movementDirection != 0 && !isAttacking && !isDead)
        {
            rb.linearVelocity = new Vector2(movementDirection * moveSpeed, rb.linearVelocity.y);
            animator?.SetFloat("Speed", Mathf.Abs(moveSpeed));
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement when attacking or idle
        }
    }

    public void SetDirection(float dir)
    {
        movementDirection = dir; // Set movement direction
        UpdateDirection(dir); // Update sprite direction
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

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (target == null)
            if (
                (gameObject.CompareTag("PlayerUnit") && collision.gameObject.CompareTag("EnemyUnit")) ||
                (gameObject.CompareTag("EnemyUnit") && collision.gameObject.CompareTag("PlayerUnit"))
                )
            {
                rb.linearVelocity = Vector2.zero; // Stop movement on collision
                target = collision.gameObject; // Set the target to the colliding enemy

                if (!isAttacking)
                {
                    StartCoroutine(Attack());
                }
            }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == target)
        {
            target = null;
            StopAllCoroutines();
            isAttacking = false;
            animator?.ResetTrigger("Attack");
        }
    }
    private IEnumerator Attack()
    {
        isAttacking = true;

        while (target != null && Vector2.Distance(transform.position, target.transform.position) <= attackRange)
        {
            _TriggerAttack();

            if (projectilePrefab == null || projectileSpawnPoint == null)
            {
                // Deal damage directly if no projectile
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
                // Spawn a projectile
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    Vector2 direction = (target.transform.position - transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, target.transform.position);
                    projectileScript.Initialize(direction, distance);
                    projectile.tag = gameObject.tag; // Match projectile tag with the unit's tag
                }
            }

            // Wait for the attack animation to complete
            yield return new WaitForSeconds(GetAnimationLength("Attack"));

            // Wait for the attack speed duration before the next attack
            yield return new WaitForSeconds(attackSpeed);
        }

        isAttacking = false;
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
        return 0.5f; // Default duration if animation not found
    }

    private void _TriggerAttack()
    {
        if (animator != null && target != null)
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
            Destroy(gameObject, GetAnimationLength("Die")); // Wait for the die animation to complete
        }
        else
        {
            Destroy(gameObject); // Fallback if no animator exists
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Update the health bar
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
