using UnityEngine;

public class Controller : MonoBehaviour
{
    //Define cual es nuestro estado actual.
    protected State currentState;

    // Update is called once per frame
    protected virtual void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdateState();
        }
    }

    public void ChangeState(State newState)
    {
        //Si existe un estado actual....
        if (currentState != null)
        {
            currentState.OnExitState(); //Nos salimos de este
        }

        currentState = newState; //Ahora el actual es el nuevo.

        //Nos metemos en el nuevo y hacemos saber cual es su controlador.
        currentState.OnEnterState(this);
    }
}
