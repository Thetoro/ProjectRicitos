using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField] 
    private LayerMask wallLayer;
    

    private Rigidbody2D rb;
    private InputAction move;
    private InputAction jump;
    private Vector2 movementDirection;
    [SerializeField]
    private Collider2D playerCollider;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [SerializeField]
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    [SerializeField] 
    private float wallCheckDistance = 0.2f;
    [SerializeField] 
    private float wallJumpForceX = 8f;
    [SerializeField] 
    private float wallJumpForceY = 12f;
    [SerializeField] 
    private float wallJumpLockTime = 0.2f;
    [SerializeField] 
    private float wallSlideSpeed = -2f;
    public float groundCheckDistance = 0.1f;

    private bool isTouchingWall;
    private bool wallOnRight;
    private bool canWallJump;
    private float lockTimer;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");
        playerCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = move.ReadValue<Vector2>();

        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
        }

        CheckWall();

        //Debug.Log("WallJump: " + canWallJump);

        if (canWallJump && jump.WasPressedThisFrame())
        {
            DoWallJump();
        }


        if (isTouchingWall && !IsGrounded() && rb.linearVelocityY < 0.15f)
        {
            Debug.Log("Deslizarse");
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Min(rb.linearVelocityY, -wallSlideSpeed));
            jumpBufferCounter = 0f;
            Debug.Log("Velcoidad Y: " + rb.linearVelocityY);
        }

        if (lockTimer > 0)
            lockTimer -= Time.deltaTime;

        if (rb.linearVelocityY < 0)
            rb.gravityScale = 4f;
        else
            rb.gravityScale = 2f;
    }

    private void FixedUpdate()
    {
        Walk();
    }

    public void Jump()
    {
        rb.AddForceAtPosition(new Vector2(0, 1) * jumpForce, Vector2.up, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    public void Walk()
    {
        //rb.MovePosition(rb.position + new Vector2(movementDirection.x, rb.position.y) * speed * Time.deltaTime);
        rb.linearVelocityX = movementDirection.x * speed;
    }

    private bool IsGrounded()
    {
        //if (playerCollider == null) return false;

        Bounds bounds = playerCollider.bounds;

        // Origen ligeramente por encima del borde inferior
        Vector2 origin = new Vector2(
            bounds.center.x,
            bounds.min.y + 0.05f
        );

        // Tamaño del box (no halfExtents en 2D)
        Vector2 size = new Vector2(
            bounds.size.x * 0.9f,
            0.1f
        );

        float distance = groundCheckDistance;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            size,
            0f,
            Vector2.down,
            distance,
            groundLayer
        );
        
        Debug.DrawRay(origin, Vector2.down * distance, hit.collider ? Color.green : Color.red);

        return hit;
    }

    private void CheckWall()
    {
        Bounds bounds = playerCollider.bounds;

        Vector2 origin = bounds.center;
        /*float dir = wallOnRight ? 1f : -1f;

        RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, wallLayer);*/

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            new Vector2(1.1f, bounds.size.y * 0.8f),
            0f,
            wallOnRight ? Vector2.right : Vector2.left,
            wallCheckDistance,
            wallLayer
        );

        /*if (hitRight.collider != null)
        {
            isTouchingWall = true;
            wallOnRight = true;
        }
        else if (hitLeft.collider != null)
        {
            isTouchingWall = true;
            wallOnRight = false;
        }
        else
        {
            isTouchingWall = false;
        }*/

        canWallJump = hit && !IsGrounded();
        isTouchingWall = hit;
    }

    private void DoWallJump()
    {
        lockTimer = wallJumpLockTime;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(
            wallOnRight ? -wallJumpForceX : wallJumpForceX,
            wallJumpForceY
        );
        jumpBufferCounter = 0f;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = playerCollider.bounds;
        Gizmos.DrawCube(transform.position, new Vector2(1.1f, bounds.size.y * 0.8f));
    }
}
