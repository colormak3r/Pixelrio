using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform background;

    // Movement variables
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float smoothSpeed = 0.125f;

    // Jump variables
    [Header("Jump")]
    public float jumpForce = 10f;
    private int extraJumps;
    public int extraJumpsValue = 1; // Number of extra jumps allowed (1 for double jump)

    private bool isGrounded;

    // Ground check variables
    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteAnimator animator;

    private Vector2 currentVelocity;
    private float moveInput_cache = 1;

    private Vector3 originalBGScale;
    private bool isJumping = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();
        extraJumps = extraJumpsValue;
        originalBGScale = background.localScale;
    }

    private void FixedUpdate()
    {
        // Handle horizontal movement
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != moveInput_cache && moveInput != 0)
            moveInput_cache = moveInput;

        var velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        rb.velocity = Vector2.SmoothDamp(rb.velocity, velocity, ref currentVelocity, smoothSpeed);


        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private float moveInput;
    private float delay;

    private void Update()
    {
        if (isJumping)
        {
            animator.ChangeAnimation("Jump");
        }
        else
        {
            if (moveInput != 0)
                animator.ChangeAnimation("Move");
            else
                animator.ChangeAnimation("Idle");
        }

        // Reset extra jumps when grounded
        if (isGrounded && Time.time > delay)
        {
            extraJumps = extraJumpsValue;
            isJumping = false;
        }


        // Handle jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (extraJumps > 0)
            {
                isJumping = true;
                delay = Time.time + 0.5f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                extraJumps--;
            }
            else if (isGrounded && extraJumps == 0)
            {
                isJumping = true;
                delay = Time.time + 0.5f;
                // Allow jumping when grounded and no extra jumps are left
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Flip the player
        transform.localScale = new Vector3(moveInput_cache, 1, 1);
        background.localScale = new Vector3(-moveInput_cache * originalBGScale.x, originalBGScale.y, originalBGScale.z);
    }
}
