using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class Room : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject enemySpawnPoints;
    public Transform playerSpawnPoint;
    public Transform cameraSpawn;
    public List<Vector3> enemySpawnPositions;
    public bool inRoom;
    public int minEnemies;
    public int maxEnemies;
    public int numWaves;
    public int currentWave;
    public List<GameObject> enemyPrefabs;
    public List<Door> doors;
    public List<GameObject> enemies;
    void Start()
    {
        enemySpawnPositions = new List<Vector3>();
        foreach(Transform transform in enemySpawnPoints.GetComponentsInChildren<Transform>())
        {
            enemySpawnPositions.Add(transform.position);
        }
        enemySpawnPositions.Remove(transform.position);
    }

    public void EnterRoom(Transform player, Transform camera, int minEnemies, int maxEnemies, int numWaves)
    {
        inRoom = true;
        camera.position = cameraSpawn.position;
        player.gameObject.GetComponent<NavMeshAgent>().enabled = false;
        player.position = playerSpawnPoint.position;
        player.gameObject.GetComponent<NavMeshAgent>().enabled = true;
        this.minEnemies = minEnemies;
        this.maxEnemies = maxEnemies;
        this.numWaves = numWaves;
        currentWave = 0;
        SetDoors(true);
    }

    public void ExitRoom(Transform player, Transform camera, int minEnemies, int maxEnemies)
    {
        inRoom = false;
    }

    public void SpawnWave()
    {
        int upperBound = Mathf.Min(maxEnemies, enemySpawnPositions.Count);
        int numToSpawn = Random.Range(minEnemies, upperBound);
        List<Vector3> spawnsToUse = new List<Vector3>(enemySpawnPositions);
        for (int i = 0; i < numToSpawn; i++)
        {
            int spawnPosIndex = Random.Range(0, spawnsToUse.Count);
            int enemyPrefabIndex = Random.Range(0, enemyPrefabs.Count);
            GameObject newEnemy = Instantiate(enemyPrefabs[enemyPrefabIndex], spawnsToUse[spawnPosIndex], Quaternion.identity);
            enemies.Add(newEnemy);
            spawnsToUse.RemoveAt(spawnPosIndex);
        }
    }

    public void SetDoors(bool locked)
    {
        foreach (Door door in doors)
        {
            door.locked = locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inRoom)
        {
            if (currentWave < numWaves && enemies.Count == 0)
            {
                currentWave++;
                SpawnWave();
            }

            for (int i = enemies.Count - 1; i > -1; i--)
            {
                if (enemies[i] == null)
                    enemies.RemoveAt(i);
            }

            if (currentWave == numWaves && enemies.Count == 0)
            {
                SetDoors(false);
            }
        }
    }
}
