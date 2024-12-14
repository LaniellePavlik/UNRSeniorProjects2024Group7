//Script: GameMgr.cs
//Contributor: Liam Francisco
//Summary: Handles transitioning between rooms and beating/failing the level
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{

    public static GameMgr inst;
    public List<int> roomIndexes;
    public List<Room> rooms;
    public int numRooms;
    public int currentRoom;
    public Room BossRoom;
    public PanelMover gameOver;
    public PanelMover gameWon;
    bool loadNext;
    bool inLastRoom;
    private void Awake()
    {
        inst = this;
    }

    // initializes the order of which rooms will appear and initialized variables for counting what room the player is in.
    void Start()
    {
        roomIndexes = new List<int>();
        for(int i = 0; i < numRooms-1; i++)
        {
            int roomIndex = Random.Range(0, rooms.Count);
            roomIndexes.Add(roomIndex);
        }
        roomIndexes.Add(rooms.Count - 1);
        currentRoom = -1;
        loadNext = true;
    }

    // checks whether or not the current room is in a valid state for the next room to be loaded.
    void Update()
    {
        if (loadNext)
        {
            NextRoom();
            loadNext = false;
        }
        if((inLastRoom && BossRoom.currentWave == BossRoom.numWaves && BossRoom.enemies.Count == 0))
        {
            gameWon.isVisible = true;
            //Time.timeScale = 0;
        }
        if (PlayerMgr.inst.player.health <= 0)
        {
            gameOver.isVisible = true;
            //Time.timeScale = 0;
        }

    }

    // loads the next room
    public void NextRoom()
    {
        currentRoom++;
        if (currentRoom != numRooms)
            rooms[roomIndexes[currentRoom]].EnterRoom(PlayerMgr.inst.player.transform, Camera.main.transform, 1, 3, 2);
        else
            EnterBossRoom();
    }

    // loads the boss room
    public void EnterBossRoom()
    {
        BossRoom.EnterRoom(PlayerMgr.inst.player.transform, Camera.main.transform, 1, 1, 1);
        inLastRoom = true;
    }
}