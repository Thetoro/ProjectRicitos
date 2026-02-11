using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

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
    private SpriteRenderer spriteRenderer;
    private Vector2 movementDirection;
    [SerializeField]
    private Collider2D playerCollider;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpContinuesForce;
    [SerializeField]
    private float acceleration = 50f;
    [SerializeField]
    private float deceleration = 40f;
    [SerializeField]
    private float airControlMultiplier = 0.5f;
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
    private bool isWallJumping; // Nueva bandera para wall jump
    private float lockTimer;
    private float currentSpeed;
    private bool isMoving;
    private float accelerationHolder;
    private float decelerationHolder;


    // Variables para guardar estado de debug
    private Bounds bounds;
    private Vector2 origin;
    private RaycastHit2D hitRight;
    private RaycastHit2D hitLeft;

    [Header("Colores Gizmo")]
    private Color rightCastColor = Color.cyan;
    private Color leftCastColor = Color.magenta;
    private Color hitColor = Color.red;
    private Color noHitColor = Color.green;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        accelerationHolder = acceleration;
        decelerationHolder = deceleration;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
        //rb.linearVelocity = new Vector2(movementDirection.x * speed, rb.linearVelocityY);
        isMoving = movementDirection.magnitude > 0.1f;
        // Actualizar dirección del sprite basado en input (no en velocidad)
        if (movementDirection.x != 0)
        {
            spriteRenderer.flipX = movementDirection.x < 0;
        }
    }


    // Update is called once per frame
    void Update()
    {
        //movementDirection = move.ReadValue<Vector2>();

        //Condicional para el Coyote Time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            isWallJumping = false; // Resetear wall jump cuando toca el suelo
        }
        else
            coyoteTimeCounter -= Time.deltaTime;

        //Condicional para el Buffer de salto
        if (jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        CheckWall();

        //Wall Jump
        if (canWallJump && jump.WasPressedThisFrame())
        {
            DoWallJump();
        }


        //Condicional para saltar
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isWallJumping)
        {
            Jump();
        }

        //Deslizarse por la pared
        if (isTouchingWall && !IsGrounded() && rb.linearVelocityY < 0.15f)
        {
            //Debug.Log("Deslizarse");
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Min(rb.linearVelocityY, -wallSlideSpeed));
            jumpBufferCounter = 0f;

        }

        if (lockTimer > 0)
            lockTimer -= Time.deltaTime;

        //Si se deja precionado el boton de salto el Player saltas mas alto
        if (jump.IsPressed() && rb.linearVelocityY > 0)
        {
            rb.AddForceY(jumpContinuesForce, ForceMode2D.Force);
            Debug.Log("Velocidad Y: " + rb.linearVelocityY);
        }
            

        if (rb.linearVelocityY < 0)
            rb.gravityScale = 4f;
        else
            rb.gravityScale = 2f;

        
    }

    void FixedUpdate()
    {
        // Solo aplicar movimiento normal si NO está en wall jump
        if (!isWallJumping || lockTimer <= 0)
        {
            ApplyMovement();
        }
    }

    private void ApplyMovement()
    {
        // Si estamos en wall jump y aún hay tiempo de bloqueo, no aplicar movimiento
        if (isWallJumping && lockTimer > 0)
            return;

        float targetSpeed = movementDirection.x * speed;
        bool isGrounded = IsGrounded();

        // Ajustar aceleración/desaceleración basado en si está en el suelo o aire
        float currentAcceleration = isGrounded ? acceleration : acceleration * airControlMultiplier;
        float currentDeceleration = isGrounded ? deceleration : deceleration * airControlMultiplier;

        if (isMoving)
        {
            // Si está en una pared, reducir el control
            if (isTouchingWall && !isGrounded)
            {
                currentAcceleration *= 0.3f;
                currentDeceleration *= 0.3f;
            }

            // Aceleración progresiva hacia la velocidad objetivo
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                currentAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Desaceleración progresiva cuando no hay input
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                currentDeceleration * Time.fixedDeltaTime
            );
        }

        // Aplicar velocidad horizontal manteniendo velocidad vertical
        Vector2 newVelocity = new Vector2(currentSpeed, rb.linearVelocityY);
        rb.linearVelocity = newVelocity;
    }


    public void Jump()
    {
        rb.AddForceAtPosition(new Vector2(0, 1) * jumpForce, Vector2.up, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
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

        // Realizar BoxCast en ambas direcciones
        RaycastHit2D hitRight = Physics2D.BoxCast(
            origin,
            new Vector2(0.2f, bounds.size.y * 0.8f),
            0f,
            Vector2.right,
            wallCheckDistance,
            wallLayer
        );

        RaycastHit2D hitLeft = Physics2D.BoxCast(
            origin,
            new Vector2(0.2f, bounds.size.y * 0.8f),
            0f,
            Vector2.left,
            wallCheckDistance,
            wallLayer
        );

        // Actualizar wallOnRight basado en las detecciones
        if (hitRight.collider != null)
        {
            isTouchingWall = true;
            wallOnRight = true;  // ¡Cambia a TRUE aquí!
            //Debug.Log("Pared detectada a la DERECHA");
        }
        else if (hitLeft.collider != null)
        {
            isTouchingWall = true;
            wallOnRight = false; // Cambia a FALSE aquí
            //Debug.Log("Pared detectada a la IZQUIERDA");
        }
        else
        {
            isTouchingWall = false;
            // No cambiamos wallOnRight cuando no hay pared
        }

        canWallJump = isTouchingWall && !IsGrounded();
        //Debug.Log("Wall on Right: " + wallOnRight);

    }

    private void DoWallJump()
    {
        isWallJumping = true;
        lockTimer = wallJumpLockTime;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(
            wallOnRight ? -wallJumpForceX : wallJumpForceX,
            wallJumpForceY
        );
        
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        Debug.Log("Fuerza WallJump: " + force);
        rb.AddForce(force, ForceMode2D.Impulse);

        // Desactivar wall jumping después del tiempo de bloqueo
        Invoke(nameof(EndWallJump), wallJumpLockTime);
    }

    private void EndWallJump()
    {
        isWallJumping = false;
        currentSpeed = rb.linearVelocityX; // Sincronizar la velocidad horizontal
        //Debug.Log("Wall Jump terminado, movimiento normal restaurado");
    }

    /*private void OnDrawGizmos()
    {
        Bounds bounds = playerCollider.bounds;

        if (!Application.isPlaying || playerCollider == null) return;

        // Dibujar el collider del jugador
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Dibujar punto de origen
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origin, 0.05f);

        // Configuración para ambos lados
        Vector2 size = new Vector2(0.2f, bounds.size.y * 0.8f);

        // --- DETECCIÓN DERECHA ---
        Vector2 rightEnd = origin + Vector2.right * wallCheckDistance;

        // Dibujar caja de origen (derecha)
        Gizmos.color = rightCastColor;
        Gizmos.DrawWireCube(origin, size);

        // Dibujar línea de detección
        Gizmos.DrawLine(origin, rightEnd);

        // Dibujar caja al final
        Gizmos.DrawWireCube(rightEnd, size);

        // Mostrar resultado
        if (hitRight.collider != null)
        {
            Gizmos.color = hitColor;
            Gizmos.DrawSphere(hitRight.point, 0.08f);

            // Dibujar línea hasta el punto de impacto
            Gizmos.DrawLine(origin, hitRight.point);

            // Dibujar caja en el punto de impacto
            Vector2 hitBoxCenter = origin + Vector2.right * hitRight.distance;
            Gizmos.DrawWireCube(hitBoxCenter, size);
        }
        else
        {
            Gizmos.color = noHitColor;
            Gizmos.DrawLine(origin, rightEnd);
        }

        // --- DETECCIÓN IZQUIERDA ---
        Vector2 leftEnd = origin + Vector2.left * wallCheckDistance;

        // Dibujar caja de origen (izquierda)
        Gizmos.color = leftCastColor;
        Gizmos.DrawWireCube(origin, size);

        // Dibujar línea de detección
        Gizmos.DrawLine(origin, leftEnd);

        // Dibujar caja al final
        Gizmos.DrawWireCube(leftEnd, size);

        // Mostrar resultado
        if (hitLeft.collider != null)
        {
            Gizmos.color = hitColor;
            Gizmos.DrawSphere(hitLeft.point, 0.08f);

            // Dibujar línea hasta el punto de impacto
            Gizmos.DrawLine(origin, hitLeft.point);

            // Dibujar caja en el punto de impacto
            Vector2 hitBoxCenter = origin + Vector2.left * hitLeft.distance;
            Gizmos.DrawWireCube(hitBoxCenter, size);
        }
        else
        {
            Gizmos.color = noHitColor;
            Gizmos.DrawLine(origin, leftEnd);
        }
    }*/
}
