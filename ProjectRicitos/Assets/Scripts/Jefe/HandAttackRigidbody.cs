using System.Collections;
using UnityEngine;

public class HandAttackRigidbody : State
{
    private HandController main;
    private Animator anim;
    private bool shake;

    private Vector3 startPosition;
    private bool isAttacking = false;
    private bool isReturning = false;
    private Coroutine attackCoroutine;

    [SerializeField]
    private float downSpeed = 5f;

    // --- Nuevo: referencia al Rigidbody2D ---
    private Rigidbody2D rb;

    public override void OnEnterState(Controller controller)
    {
        main = controller as HandController;
        startPosition = transform.position;
        isAttacking = false;
        isReturning = false;
        shake = false;

        // Obtener o añadir el Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // Configuración típica: cinemático si controlas la posición, o dinámico con gravedad 0
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // Si quieres que el movimiento sea totalmente controlado por MovePosition, usa isKinematic = true
        // rb.isKinematic = true; // opcional, pero si lo pones, las colisiones seguirán funcionando
    }

    public override void OnUpdateState()
    {
        // En Update solo lanzamos la lógica de ataque (no movemos directamente)
        // El movimiento se hará en FixedUpdate mediante el Rigidbody
        LaunchAttack();
    }

    // --- Nuevo: FixedUpdate para mover con física ---
    private void FixedUpdate()
    {
        if (isReturning)
        {
            // Fase de retorno: mover hacia la posición original con MovePosition
            Vector2 targetPos = startPosition;
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, downSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, targetPos) < 0.01f)
            {
                // Llegamos al origen: finalizamos el ataque
                main.ChangeState(main.PatrolState);
            }
        }
        else if (isAttacking)
        {
            // Fase de ataque: bajar hacia el objetivo (y = 0.5f)
            Vector2 target = new Vector2(transform.position.x, 0.5f);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, downSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Mathf.Approximately(rb.position.y, target.y) && !shake)
            {
                // Activamos el temblor y la espera
                CameraShake.Instance.ShakeCamera(1.5f, 2.5f, 0.5f);
                shake = true;
                // No es necesario cambiar isAttacking aquí, ya está en true
                // Iniciamos la corrutina para el retorno
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(WaitAndReturn());
            }
        }
    }

    private void LaunchAttack()
    {
        // Esta función solo inicia el ataque si no está en curso
        if (!isReturning && !isAttacking)
        {
            // Marcar que estamos atacando (la física se encargará de mover)
            isAttacking = true;
            // La posición objetivo se evaluará en FixedUpdate
        }
    }

    public override void OnExitState()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        // Opcional: resetear flags
        isAttacking = false;
        isReturning = false;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Player player))
        {
            main.ChangeState(main.PatrolState);
        }
    }*/

    IEnumerator WaitAndReturn()
    {
        yield return new WaitForSeconds(2f);
        isReturning = true;    // Ahora en FixedUpdate se moverá hacia startPosition
        isAttacking = false;
        attackCoroutine = null;
        shake = false;
    }
}
