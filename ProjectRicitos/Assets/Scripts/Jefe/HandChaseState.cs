using UnityEngine;

public class HandChaseState : State
{
    private HandController main;

    private HandPatrolState limit;

    [SerializeField]
    private float chaseVelocity;
    [SerializeField]
    private float timeToAttack;

    private float originalTime;

    private void Start()
    {
        limit = GetComponent<HandPatrolState>();
        originalTime = timeToAttack;
    }

    public override void OnEnterState(Controller controller)
    {
        main = controller as HandController;
        timeToAttack = originalTime;

    }
    public override void OnUpdateState()
    {
        ChaseTarget();
    }

    private void ChaseTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(main.HandTarget.position.x, transform.position.y), chaseVelocity * Time.deltaTime);
        EnfoqueDestino();
        timeToAttack -= Time.deltaTime;

        if(transform.position.x >= limit.PointA.position.x)
        {
            transform.position = new Vector3(limit.PointA.position.x, transform.position.y);
        }

        if(transform.position.x <= limit.PointB.position.x)
        {
            transform.position = transform.position = new Vector3(limit.PointB.position.x, transform.position.y);
        }

        if (timeToAttack <= 0)
        {
            main.ChangeState(main.AttackState);
        }
    }

    public override void OnExitState()
    {

    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            main.ChangeState(main.PatrolState);
        }
    }

    private void EnfoqueDestino()
    {
        if (main.HandTarget.position.x > transform.position.x)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
