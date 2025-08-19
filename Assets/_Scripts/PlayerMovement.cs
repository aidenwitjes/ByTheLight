using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask = -1;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D col;

    // Input variables
    private float moveInput;
    private bool jumpInputDown;
    private bool jumpInput;

    // Ground detection
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool hasJumped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 2.5f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (groundCheck == null)
        {
            GameObject groundCheckGO = new GameObject("GroundCheck");
            groundCheckGO.transform.SetParent(transform);
            groundCheckGO.transform.localPosition = new Vector3(0, -col.bounds.extents.y - 0.1f, 0);
            groundCheck = groundCheckGO.transform;
        }
    }

    private void Update()
    {
        HandleInput();
        CheckGroundStatus();
        HandleCoyoteTime();
        HandleJumpBuffer();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJumping();
    }

    private void HandleInput()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        jumpInputDown = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);
        jumpInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);
    }

    private void HandleMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    private void HandleJumping()
    {
        // Can only jump if: have jump buffer, have coyote time, and not already jumping/moving up
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !hasJumped && rb.linearVelocity.y <= 0.1f)
        {
            Jump();
            jumpBufferCounter = 0f;
            hasJumped = true;
        }

        // Variable jump height
        if (!jumpInput && rb.linearVelocity.y > 0f)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y *= 0.5f;
            rb.linearVelocity = velocity;
        }
    }

    private void Jump()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
        coyoteTimeCounter = 0f; // consume coyote time
    }

    private void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }

    private void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            hasJumped = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJumpBuffer()
    {
        if (jumpInputDown)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null && moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
