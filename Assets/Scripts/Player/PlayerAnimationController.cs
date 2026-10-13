using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    [Header("Facing Direction")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private float directionThreshold = 0.1f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        HandleFacingDirection();
    }

    private void HandleFacingDirection()
    {
        if (spriteRenderer == null || rb == null)
            return;

        float horizontalVelocity = rb.linearVelocity.x;

        // Keep the last direction when standing still.
        if (Mathf.Abs(horizontalVelocity) < directionThreshold)
            return;

        bool movingRight = horizontalVelocity > 0f;

        // Flip based on the sprite's original direction.
        spriteRenderer.flipX = movingRight != spriteFacesRight;
    }
}