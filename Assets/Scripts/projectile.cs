using UnityEngine;

public class Projectile : MonoBehaviour
{
    public AnimationCurve heightCurve;
    public int damage = 10;
    public float speed = 10f;
    public float lifetime = 2f; // Time before the projectile is destroyed
    private Vector2 direction;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float elapsedTime = 0f;

    public void Initialize(Vector2 dir, float distance)
    {
        startPosition = transform.position;
        targetPosition = startPosition + (dir.normalized * distance);

        Destroy(gameObject, lifetime); // Destroy the projectile after its lifetime
    }

    void Update()
    {

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / lifetime);

        // Calculate position based on progress and curve
        float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
        float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress) + heightCurve.Evaluate(progress);

        transform.position = new Vector2(x, y);
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
