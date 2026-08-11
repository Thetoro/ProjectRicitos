using UnityEngine;

public class HandPatrolState : State
{
    private HandController main;

    //private Animator anim;

    [SerializeField]
    private Transform pointA, pointB;

    [SerializeField]
    private float patrolVelocity;


    private Transform currentDestination;

    public Transform PointA { get => pointA; }
    public Transform PointB { get => pointB; }

    public override void OnEnterState(Controller controller)
    {
        main = controller as HandController;

        currentDestination = pointA;


        //anim = GetComponentInChildren<Animator>();

        //anim.SetBool("atacar", false);
    }

    public override void OnUpdateState()
    {
        PatrolBetweenPoints();
    }

    private void PatrolBetweenPoints()
    {
        //Nos vamos moviendo...
        transform.position = Vector3.MoveTowards(transform.position, currentDestination.position, patrolVelocity * Time.deltaTime);

        if (transform.position == currentDestination.position) //Si llegamos al destino...
        {
            //Cambiamos. Si tenemos como destino A, pasamos a B y viceversa. (Operador ternario)
            currentDestination = currentDestination == pointA ? pointB : pointA;
        }

    }

    public override void OnExitState()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            main.HandTarget = player.transform;
            main.ChangeState(main.ChaseState);
        }
    }

}
