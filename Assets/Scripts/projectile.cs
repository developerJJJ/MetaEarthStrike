using UnityEngine;

public class Projectile : MonoBehaviour
{
    public AnimationCurve heightCurve;
    public int damage = 10;
    public float speed = 10f;
    public float lifetime = 1f; // Time before the projectile is destroyed
    private Vector2 direction;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float elapsedTime = 0f;

    public void Initialize(Vector2 dir, float distance)
    {
        startPosition = transform.position;
        targetPosition = startPosition + (dir.normalized * distance);
        direction = dir.normalized;

        Destroy(gameObject, lifetime); // Destroy the projectile after its lifetime
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / lifetime);

        // Calculate position based on progress and curve
        float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
        float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress) + 2.5f * heightCurve.Evaluate(progress);

        Vector2 currentPosition = new Vector2(x, y);

        // Update position
        transform.position = currentPosition;

        // Rotate to face target
        if (progress < 1f)
        {
            Vector2 lookDirection = (targetPosition - currentPosition).normalized;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Unit targetUnit = collision.GetComponent<Unit>();
        if (targetUnit != null && collision.gameObject.CompareTag(gameObject.CompareTag("PlayerUnit") ? "EnemyUnit" : "PlayerUnit"))
        {
            targetUnit.TakeDamage(damage);
            Destroy(gameObject); // Destroy the projectile upon impact
        }
    }
}