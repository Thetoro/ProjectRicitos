using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    private GameObject player;

    private Transform checkPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        checkPoint = startPoint;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void UpdateCheckPoint(Transform newSpawnPoint)
    {
        checkPoint = newSpawnPoint;

    }

    public void RespawnPlayer()
    { 
        StartCoroutine(SpawnPlayer());
    }

    IEnumerator SpawnPlayer()
    {
        //Animacion de Spawn
        yield return new WaitForSeconds(1f);
        player.transform.position = checkPoint.position;
        player.SetActive(true);

    }
}
