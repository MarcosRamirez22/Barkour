
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Facing Direction")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private float directionThreshold = 0.1f;

    private InputAction moveAction;

    private static readonly int IsWallSlidingHash =
        Animator.StringToHash("IsWallSliding");

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void LateUpdate()
    {
        HandleFacingDirection();
    }

    private void HandleFacingDirection()
    {
        if (spriteRenderer == null || rb == null)
            return;

        // Wall animation takes priority over velocity.
        bool isWallAnimating =
            animator != null &&
            animator.GetBool(IsWallSlidingHash);

        if (isWallAnimating && moveAction != null)
        {
            float inputX = moveAction.ReadValue<Vector2>().x;

            // Face the direction the player is pressing.
            if (Mathf.Abs(inputX) >= directionThreshold)
            {
                bool facingRight = inputX > 0f;
                spriteRenderer.flipX =
                    facingRight != spriteFacesRight;
            }

            // Ignore collision velocity while against a wall.
            return;
        }

        // Normal movement uses horizontal velocity.
        float horizontalVelocity = rb.linearVelocity.x;

        // Keep the last direction when moving slowly.
        if (Mathf.Abs(horizontalVelocity) < directionThreshold)
            return;

        bool movingRight = horizontalVelocity > 0f;

        spriteRenderer.flipX =
            movingRight != spriteFacesRight;
    }
}
