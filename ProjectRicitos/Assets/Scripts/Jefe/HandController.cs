using UnityEngine;

/// <summary>
/// En esta clase se definen los datos compartidos entre estados y los estados.
/// </summary>
public class HandController : Controller
{
    [SerializeField]
    private float attackRange;  //Dato compartido por varios estados: Attack y Chase.

    [SerializeField]
    private SpriteRenderer batVisual;

    private Transform handTarget;  //Datos compartido por varios estados: Patrol y Chase.



    //Se definen los estados que va a tener el enemigo:
    private HandPatrolState patrolState;
    private HandChaseState chaseState;
    private HandAttackState attackState;


    #region getters & setters
    public HandPatrolState PatrolState { get => patrolState; }
    public HandChaseState ChaseState { get => chaseState; }
    public HandAttackState AttackState { get => attackState; }
    public Transform HandTarget { get => handTarget; set => handTarget = value; }
    public float AttackRange { get => attackRange; }
    public SpriteRenderer BatVisual { get => batVisual; }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        InitStates();

        ChangeState(patrolState);
    }


    //Inicializa los estados pasando como parametro el controlador al que pertenecen.
    private void InitStates()
    {
        patrolState = GetComponent<HandPatrolState>();
        chaseState = GetComponent<HandChaseState>();
        attackState = GetComponent<HandAttackState>();
    }

    protected override void Update()
    {
        base.Update();
    }
}
