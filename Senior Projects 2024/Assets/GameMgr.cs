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
    bool loadNext;
    bool inLastRoom;
    private void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        if (loadNext)
        {
            NextRoom();
            loadNext = false;
        }
        if((inLastRoom && BossRoom.currentWave == BossRoom.numWaves && BossRoom.enemies.Count == 0) || PlayerMgr.inst.player.health <= 0)
        {
            //game over
        }

    }

    public void NextRoom()
    {
        currentRoom++;
        if (currentRoom != numRooms)
            rooms[roomIndexes[currentRoom]].EnterRoom(PlayerMgr.inst.player.transform, Camera.main.transform, 1, 3, 2);
        else
            EnterBossRoom();
    }

    public void EnterBossRoom()
    {
        BossRoom.EnterRoom(PlayerMgr.inst.player.transform, Camera.main.transform, 1, 1, 1);
        inLastRoom = true;
    }
}