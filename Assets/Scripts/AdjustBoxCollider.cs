using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class AdjustBoxCollider : MonoBehaviour
{
    void Start()
    {
        // Get components
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // Adjust size of BoxCollider2D to match sprite size
        boxCollider.size = spriteRenderer.sprite.bounds.size;
        boxCollider.offset = spriteRenderer.sprite.bounds.center;
    }
}
