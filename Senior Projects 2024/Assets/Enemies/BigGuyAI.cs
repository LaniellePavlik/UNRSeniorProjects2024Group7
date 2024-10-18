using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BigGuyAI : EnemyAI
{
    public Transform player;
    public float BGMaxStopTime;
    public float BGMinStopTime;
    public float BGMaxMoveTime;
    public float BGMinMoveTime;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        maxStopTime = BGMaxStopTime;
        minStopTime = BGMinStopTime;
        maxMoveTime = BGMaxMoveTime;
        minMoveTime = BGMinMoveTime;

        moveTime = Random.Range(minMoveTime, maxMoveTime);
        stopTime = Random.Range(minStopTime, maxStopTime);
    }

    // Update is called once per frame
    void Update()
    {
        SetMove(player.position);
        StopInterval();
    }
}
