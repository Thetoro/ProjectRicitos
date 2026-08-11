using UnityEngine;

public abstract class State : MonoBehaviour
{
    //Todo estado tendr・tres fases: Entrada, actualizaci y salida.
    public abstract void OnEnterState(Controller controller);

    public abstract void OnUpdateState();

    public abstract void OnExitState();

}
