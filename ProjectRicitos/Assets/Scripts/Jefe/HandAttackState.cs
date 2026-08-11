using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandAttackState : State
{
    private HandController main;
    private Animator anim;
    private bool shake;

    private Vector3 startPosition;   // posición original
    private bool isAttacking = false; // indica si ya se está ejecutando el ataque
    private bool isReturning = false; // indica si está en fase de subida
    private Coroutine attackCoroutine;

    [SerializeField]
    private float downSpeed = 5f;

    public override void OnEnterState(Controller controller)
    {
        main = controller as HandController;
        startPosition = transform.position;   // guardamos la posición de inicio
        isAttacking = false;
        isReturning = false;
        shake = false;

        //main.BatVisual.color = Color.red; //Para ver visualmente cuando cambia de un estado a otro.

        //fanim = GetComponentInChildren<Animator>();
    }
    public override void OnUpdateState()
    {

        LaunchAttack();

        /*if (Vector3.Distance(transform.position, main.HandTarget.position) > main.AttackRange)
        {
            main.ChangeState(main.ChaseState);
        }*/
    }

    private void LaunchAttack()
    {
        if (isReturning)
        {
            // Fase de retorno: mover hacia la posición original
            transform.position = Vector3.MoveTowards(transform.position, startPosition, downSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, startPosition) < 0.01f)
            {
                // Llegamos al origen: finalizamos el ataque y cambiamos de estado
                main.ChangeState(main.PatrolState); // o el estado que corresponda
            }
        }
        else if (!isAttacking)
        {
            // Fase de ataque: bajar hacia el objetivo
            Vector3 target = new Vector3(transform.position.x, 0.5f);
            transform.position = Vector3.MoveTowards(transform.position, target, downSpeed * Time.deltaTime);

            if (transform.position.y == target.y && !shake)
            {
                CameraShake.Instance.ShakeCamera(1.5f, 2.5f, 0.5f);
                shake = true;
                isAttacking = true;

                // Iniciamos la corrutina que espera 1 segundo y luego activa el retorno
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(WaitAndReturn());
            }
        }
    }

    public override void OnExitState()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.TryGetComponent(out Player player))
        {
            main.ChangeState(main.PatrolState);
        }
    }*/

    private IEnumerator WaitAndReturn()
    {
        yield return new WaitForSeconds(2f);
        isReturning = true;    // ahora comenzará a subir en Update
        isAttacking = false;   // ya no estamos en la fase de ataque
        attackCoroutine = null;
        shake = false;
    }


}
