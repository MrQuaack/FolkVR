using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    public GameObject[] wallPrefabs; 
    public Transform spawnPoint;
    private int currentWallIndex = 0;

    void Start()
    {
        SpawnNextWall();
    }

    public void SpawnNextWall()
    {
        if (currentWallIndex < wallPrefabs.Length)
        {
            Instantiate(wallPrefabs[currentWallIndex], spawnPoint.position, spawnPoint.rotation);
            currentWallIndex++;
        }

    }
}