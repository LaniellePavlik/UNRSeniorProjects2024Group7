using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BigGuyAI : EnemyAI
{
    public Transform player;
    public float moveTimer = 0;
    private float moveTime;
    private float stopTime;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        moveTime = Random.Range(9, 12);
        stopTime = Random.Range(1, 3);
    }

    // Update is called once per frame
    void Update()
    {
        SetMove(player.position);
        moveTimer += Time.deltaTime;
        if (moveTimer > moveTime && !agent.isStopped)
        {
            agent.isStopped = true;
            moveTime = Random.Range(9, 12);
            moveTimer = 0;
        }
        if (moveTimer > stopTime && agent.isStopped)
        {
            agent.isStopped = false;
            stopTime = Random.Range(1, 3);
            moveTimer = 0;
        }
    }
}
