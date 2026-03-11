using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private SpawnManager spawnManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (spawnManager != null)
        {
            if(collision.CompareTag("Player"))
            {
                spawnManager.UpdateCheckPoint(this.transform);
            }
        }
    }
}
