using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 7f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float airAcceleration = 12f;
    [SerializeField] private float crouchAirAcceleration = 5f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float slideJumpSpeedThreshold = 5f;
    [SerializeField] private float slideJumpMinimumDuration = 0.33f;
    [SerializeField] private float slideJumpBoost = 1f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Jump Assistance")]
    [SerializeField] private float coyoteTime = 0.05f;
    [SerializeField] private float jumpBufferTime = 0.05f;

    [Header("Crouching")]
    [SerializeField] private float crouchHeightMultiplier = 0.5f;
    [SerializeField] private float crouchMoveSpeed = 2f;

    [Header("Sliding")]
    [SerializeField] private float minimumSlideSpeed = 4f;
    [SerializeField] private float slideDeceleration = 1f;
    [SerializeField] private float slideEndSpeed = 2f;
    [SerializeField] private float slideLockDuration = 0.15f;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private float groundCheckInset = 0.1f;
    [SerializeField] private float wallCheckDistance = 0.08f;
    [SerializeField, Range(0.1f, 0.45f)]
    private float wallCheckVerticalInset = 0.2f;

    [Header("Wall Movement")]
    [SerializeField] private float wallSlideSpeed = 2f;

    [Header("Wall Jumping")]
    [SerializeField] private float wallJumpHorizontalForce = 7f;
    [SerializeField] private float wallJumpVerticalForce = 10f;
    [SerializeField] private float wallJumpControlLockDuration = 0.15f;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    private Vector2 moveInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    private bool isGrounded;
    private bool isCrouching;
    private bool isSliding;

    private bool isTouchingWall;
    private bool isWallSliding;

    // -1 = left wall, 1 = right wall
    private int wallDirection;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float slideTimer;
    private float wallJumpControlLockCounter;

    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[3];
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[3];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");

        standingColliderSize = playerCollider.size;
        standingColliderOffset = playerCollider.offset;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        CheckGrounded();
        CheckWall();

        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleWallJumpControlLock();
        HandleCrouch();

        TryJump();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleWallSlide();
        HandleGravity();
    }

    private void CheckGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        float inset = Mathf.Min(
            groundCheckInset,
            bounds.extents.x * 0.9f
        );

        float leftX = bounds.min.x + inset;
        float centerX = bounds.center.x;
        float rightX = bounds.max.x - inset;

        float startY = bounds.min.y + 0.01f;

        Vector2 leftOrigin = new Vector2(leftX, startY);
        Vector2 centerOrigin = new Vector2(centerX, startY);
        Vector2 rightOrigin = new Vector2(rightX, startY);

        // Downward casts from inside the collider prevent nearby wall
        // corners from being interpreted as ground.
        groundHits[0] = Physics2D.Raycast(
            leftOrigin,
            Vector2.down,
            groundCheckDistance,
            environmentLayer
        );

        groundHits[1] = Physics2D.Raycast(
            centerOrigin,
            Vector2.down,
            groundCheckDistance,
            environmentLayer
        );

        groundHits[2] = Physics2D.Raycast(
            rightOrigin,
            Vector2.down,
            groundCheckDistance,
            environmentLayer
        );

        isGrounded =
            groundHits[0].collider != null ||
            groundHits[1].collider != null ||
            groundHits[2].collider != null;
    }

    private void CheckWall()
    {
        isTouchingWall = false;
        wallDirection = 0;

        if (isGrounded)
        {
            return;
        }

        Bounds bounds = playerCollider.bounds;

        float insetAmount =
            bounds.size.y * wallCheckVerticalInset;

        float bottomY = bounds.min.y + insetAmount;
        float middleY = bounds.center.y;
        float topY = bounds.max.y - insetAmount;

        float leftStartX = bounds.min.x + 0.01f;
        float rightStartX = bounds.max.x - 0.01f;

        bool touchingRightWall = CheckWallSide(
            rightStartX,
            bottomY,
            middleY,
            topY,
            Vector2.right
        );

        bool touchingLeftWall = CheckWallSide(
            leftStartX,
            bottomY,
            middleY,
            topY,
            Vector2.left
        );

        if (touchingRightWall)
        {
            isTouchingWall = true;
            wallDirection = 1;
        }
        else if (touchingLeftWall)
        {
            isTouchingWall = true;
            wallDirection = -1;
        }
    }

    private bool CheckWallSide(
        float startX,
        float bottomY,
        float middleY,
        float topY,
        Vector2 direction)
    {
        // Three short horizontal casts verify that geometry is beside
        // the player instead of allowing a ceiling overlap to count as a wall.
        wallHits[0] = Physics2D.Raycast(
            new Vector2(startX, bottomY),
            direction,
            wallCheckDistance,
            environmentLayer
        );

        wallHits[1] = Physics2D.Raycast(
            new Vector2(startX, middleY),
            direction,
            wallCheckDistance,
            environmentLayer
        );

        wallHits[2] = Physics2D.Raycast(
            new Vector2(startX, topY),
            direction,
            wallCheckDistance,
            environmentLayer
        );

        return
            wallHits[0].collider != null ||
            wallHits[1].collider != null ||
            wallHits[2].collider != null;
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJumpBuffer()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleWallJumpControlLock()
    {
        if (wallJumpControlLockCounter > 0f)
        {
            wallJumpControlLockCounter -= Time.deltaTime;
        }
    }

    private void TryJump()
    {
        if (jumpBufferCounter <= 0f)
        {
            return;
        }

        bool pressingTowardWall =
            (wallDirection == 1 && moveInput.x > 0.01f) ||
            (wallDirection == -1 && moveInput.x < -0.01f);

        if (!isGrounded &&
            isTouchingWall &&
            pressingTowardWall)
        {
            WallJump();

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            return;
        }

        if (coyoteTimeCounter > 0f)
        {
            Jump();

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void HandleCrouch()
    {
        if (crouchAction.WasPressedThisFrame())
        {
            if (!isCrouching)
            {
                StartCrouch();
            }

            if (!isSliding &&
                isGrounded &&
                Mathf.Abs(rb.linearVelocity.x) >= minimumSlideSpeed)
            {
                StartSlide();
            }
        }

        if (isSliding)
        {
            if (slideTimer < slideLockDuration)
            {
                return;
            }

            if (!crouchAction.IsPressed() && CanStand())
            {
                StopSlide();
            }

            return;
        }

        if (crouchAction.IsPressed())
        {
            if (!isCrouching)
            {
                StartCrouch();
            }

            return;
        }

        if (isCrouching && CanStand())
        {
            StopCrouch();
        }
    }

    private void StartCrouch()
    {
        isCrouching = true;

        float crouchingHeight =
            standingColliderSize.y * crouchHeightMultiplier;

        float heightDifference =
            standingColliderSize.y - crouchingHeight;

        playerCollider.size = new Vector2(
            standingColliderSize.x,
            crouchingHeight
        );

        playerCollider.offset = new Vector2(
            standingColliderOffset.x,
            standingColliderOffset.y - heightDifference / 2f
        );
    }

    private void StopCrouch()
    {
        isCrouching = false;

        playerCollider.size = standingColliderSize;
        playerCollider.offset = standingColliderOffset;
    }

    private bool CanStand()
    {
        float crouchingHeight = playerCollider.size.y;

        float heightToCheck =
            standingColliderSize.y - crouchingHeight;

        if (heightToCheck <= 0f)
        {
            return true;
        }

        Vector2 checkCenter =
            (Vector2)transform.TransformPoint(playerCollider.offset)
            + Vector2.up *
            (crouchingHeight / 2f + heightToCheck / 2f);

        Vector2 checkSize = new Vector2(
            standingColliderSize.x * 0.9f,
            heightToCheck
        );

        Collider2D ceiling = Physics2D.OverlapBox(
            checkCenter,
            checkSize,
            0f,
            environmentLayer
        );

        return ceiling == null;
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = 0f;
    }

    private void StopSlide()
    {
        isSliding = false;
        slideTimer = 0f;

        if (!crouchAction.IsPressed() &&
            isCrouching &&
            CanStand())
        {
            StopCrouch();
        }
    }

    private void HandleMovement()
    {
        if (wallJumpControlLockCounter > 0f)
        {
            return;
        }

        if (isSliding && isGrounded)
        {
            HandleSlide();
            return;
        }

        float currentMaxSpeed = maxMoveSpeed;

        if (isCrouching && isGrounded)
        {
            currentMaxSpeed = crouchMoveSpeed;
        }

        float targetSpeed =
            moveInput.x * currentMaxSpeed;

        float accelerationRate;

        if (!isGrounded)
        {
            accelerationRate = isCrouching
                ? crouchAirAcceleration
                : airAcceleration;
        }
        else if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelerationRate = acceleration;
        }
        else
        {
            accelerationRate = deceleration;
        }

        float newXVelocity = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newXVelocity,
            rb.linearVelocity.y
        );
    }

    private void HandleSlide()
    {
        slideTimer += Time.fixedDeltaTime;

        float newXVelocity = Mathf.MoveTowards(
            rb.linearVelocity.x,
            0f,
            slideDeceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newXVelocity,
            rb.linearVelocity.y
        );

        if (Mathf.Abs(newXVelocity) <= slideEndSpeed)
        {
            StopSlide();
        }
    }

    private void HandleWallSlide()
    {
        isWallSliding = false;

        if (isGrounded || !isTouchingWall)
        {
            return;
        }

        if (wallJumpControlLockCounter > 0f)
        {
            return;
        }

        if (rb.linearVelocity.y >= 0f)
        {
            return;
        }

        bool pressingTowardWall =
            (wallDirection == 1 && moveInput.x > 0.01f) ||
            (wallDirection == -1 && moveInput.x < -0.01f);

        if (!pressingTowardWall)
        {
            return;
        }

        isWallSliding = true;

        if (rb.linearVelocity.y < -wallSlideSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -wallSlideSpeed
            );
        }
    }

    private void HandleGravity()
    {
        if (isWallSliding)
        {
            return;
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (fallGravityMultiplier - 1f)
                * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f &&
                 !jumpAction.IsPressed())
        {
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (lowJumpGravityMultiplier - 1f)
                * Time.fixedDeltaTime;
        }
    }

    private void Jump()
    {
        float currentJumpForce = jumpForce;
        float horizontalSpeed =
            Mathf.Abs(rb.linearVelocity.x);

        bool qualifiesForSlideJumpBoost =
            isSliding &&
            slideTimer >= slideJumpMinimumDuration &&
            horizontalSpeed >= slideJumpSpeedThreshold;

        if (qualifiesForSlideJumpBoost)
        {
            currentJumpForce += slideJumpBoost;
        }

        if (isSliding)
        {
            isSliding = false;
            slideTimer = 0f;
        }

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            currentJumpForce
        );
    }

    private void WallJump()
    {
        isSliding = false;
        slideTimer = 0f;

        if (isCrouching && CanStand())
        {
            StopCrouch();
        }

        float jumpDirection = -wallDirection;

        rb.linearVelocity = new Vector2(
            jumpDirection * wallJumpHorizontalForce,
            wallJumpVerticalForce
        );

        wallJumpControlLockCounter =
            wallJumpControlLockDuration;

        isWallSliding = false;
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D collider = playerCollider;

        if (collider == null)
        {
            collider = GetComponent<BoxCollider2D>();
        }

        if (collider == null)
        {
            return;
        }

        Bounds bounds = collider.bounds;

        float groundInset = Mathf.Min(
            groundCheckInset,
            bounds.extents.x * 0.9f
        );

        float groundY = bounds.min.y + 0.01f;

        Vector2 groundLeft = new Vector2(
            bounds.min.x + groundInset,
            groundY
        );

        Vector2 groundCenter = new Vector2(
            bounds.center.x,
            groundY
        );

        Vector2 groundRight = new Vector2(
            bounds.max.x - groundInset,
            groundY
        );

        Gizmos.DrawLine(
            groundLeft,
            groundLeft + Vector2.down * groundCheckDistance
        );

        Gizmos.DrawLine(
            groundCenter,
            groundCenter + Vector2.down * groundCheckDistance
        );

        Gizmos.DrawLine(
            groundRight,
            groundRight + Vector2.down * groundCheckDistance
        );

        float verticalInset =
            bounds.size.y * wallCheckVerticalInset;

        float bottomY = bounds.min.y + verticalInset;
        float middleY = bounds.center.y;
        float topY = bounds.max.y - verticalInset;

        float leftX = bounds.min.x + 0.01f;
        float rightX = bounds.max.x - 0.01f;

        DrawWallGizmo(
            new Vector2(leftX, bottomY),
            Vector2.left
        );

        DrawWallGizmo(
            new Vector2(leftX, middleY),
            Vector2.left
        );

        DrawWallGizmo(
            new Vector2(leftX, topY),
            Vector2.left
        );

        DrawWallGizmo(
            new Vector2(rightX, bottomY),
            Vector2.right
        );

        DrawWallGizmo(
            new Vector2(rightX, middleY),
            Vector2.right
        );

        DrawWallGizmo(
            new Vector2(rightX, topY),
            Vector2.right
        );
    }

    private void DrawWallGizmo(
        Vector2 origin,
        Vector2 direction)
    {
        Gizmos.DrawLine(
            origin,
            origin + direction * wallCheckDistance
        );
    }
}