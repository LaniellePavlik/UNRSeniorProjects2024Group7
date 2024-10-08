using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BigGuyAI : EnemyAI
{
    public Transform player;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private float moveTimer = 0;
    private bool moving;
    public override void Move()
    {
        agent.destination = player.position;
        moveTimer += Time.deltaTime;
        if (moveTimer > 5 && moving) 
        {
            agent.isStopped = true;
            moveTimer = 0;
            moving = false;
        }
        if (moveTimer > 2 && !moving) 
        {
            agent.isStopped = false;
            moveTimer = 0;
            moving = true;
        }

        Debug.Log(Random.insideUnitCircle);
    }
}
