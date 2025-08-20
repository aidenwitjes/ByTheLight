using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LadderClimber : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 4f;

    private Rigidbody2D rb;
    private bool isOnLadder;
    private bool isClimbing;

    private float verticalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical"); // W/S or Up/Down

        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }
        else if (!isOnLadder)
        {
            isClimbing = false;
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            // Cancel gravity while climbing
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
        }
        else
        {
            // Restore gravity when not climbing
            rb.gravityScale = 2.5f; // match your PlayerMovement setting
        }
    }

    public void SetOnLadder(bool onLadder, Ladder ladder)
    {
        isOnLadder = onLadder;
        if (!onLadder)
        {
            isClimbing = false;
        }
    }
}
