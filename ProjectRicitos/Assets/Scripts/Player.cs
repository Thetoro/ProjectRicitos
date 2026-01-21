using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;
    [SerializeField]
    private GameObject groundCheck;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField] 
    private LayerMask wallLayer;
    

    private Rigidbody2D rb;
    private InputAction move;
    private InputAction jump;
    private Vector2 movementDirection;
    private Collider2D playerCollider;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float jumpForce;
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
        
        if(jump.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        CheckWall();

        //Debug.Log("WallJump: " + canWallJump);

        if (canWallJump && jump.WasPressedThisFrame())
        {
            Debug.Log("Brincar");
            DoWallJump();
        }

        /*Debug.Log("IsTouchingWall: " + isTouchingWall);
        Debug.Log("TocaPiso: " + IsGrounded());
        Debug.Log("Velocidad Y: " + rb.linearVelocityY);*/

        if (isTouchingWall && !IsGrounded())
        {
            Debug.Log("Deslizarse");
            rb.linearVelocity = new Vector2(rb.linearVelocityX, wallSlideSpeed);
        }

        if (lockTimer > 0)
            lockTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Walk();
    }

    public void Jump()
    {
        rb.AddForceAtPosition(new Vector2(0, 1) * jumpForce, Vector2.up, ForceMode2D.Impulse);
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
            new Vector2(0.1f, bounds.size.y * 0.8f),
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

        Debug.Log("isTouchingWall: " + hit.ToString());
        canWallJump = hit && !IsGrounded();
    }

    private void DoWallJump()
    {
        lockTimer = wallJumpLockTime;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(
            wallOnRight ? -wallJumpForceX : wallJumpForceX,
            wallJumpForceY
        );

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        
    }
}
