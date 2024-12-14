//Script: Room.cs
//Contributor: Liam Francisco
//Summary: Class for any rooms in a combat world
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

    //initializes all the potential positions an enemy can spawn in
    void Start()
    {
        enemySpawnPositions = new List<Vector3>();
        foreach(Transform transform in enemySpawnPoints.GetComponentsInChildren<Transform>())
        {
            enemySpawnPositions.Add(transform.position);
        }
        enemySpawnPositions.Remove(transform.position);
    }

    //spawns the player in the room and initializes the current wave to zero
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

    //spawns a new wave in the room after a previous wave has been defeated
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

    //locks/unlocks all the doors in the room allowing/preventing the player from interacting with the door
    public void SetDoors(bool locked)
    {
        foreach (Door door in doors)
        {
            door.locked = locked;
        }
    }

    // checks whether or not to spawn a new wave of enemies and whether or not to unlock the doors in the room
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
