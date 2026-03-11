using System.Collections;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    [SerializeField]
    private SpawnManager spawnManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        { 
            collision.gameObject.SetActive(false);
            spawnManager.RespawnPlayer();
        }
    }
}
