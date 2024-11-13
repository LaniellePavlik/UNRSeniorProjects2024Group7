using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AOE : EnemyAI
{
    public Transform player;
    public float AOEMaxStopTime;
    public float AOEMinStopTime;
    public float AOEMaxMoveTime;
    public float AOEMinMoveTime;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerMgr.inst.player.transform;
        movePosition = player.position;
        agent = GetComponent<NavMeshAgent>();
        maxStopTime = AOEMaxStopTime;
        minStopTime = AOEMinStopTime;
        maxMoveTime = AOEMaxMoveTime;
        minMoveTime = AOEMinMoveTime;

        moveTime = Random.Range(minMoveTime, maxMoveTime);
        stopTime = Random.Range(minStopTime, maxStopTime);
    }

    public float attackCooldown = 0;

    Vector3 movePosition;
    // Update is called once per frame
    void Update()
    {
        
        float dist = Vector3.Distance(transform.position, movePosition);
        if(dist < 1)
        {
            movePosition = player.position + GetRandomPointOnRadius(10);
        }
        SetMove(movePosition);
        StopInterval();
        
        attackCooldown += Time.deltaTime;

        if (attackCooldown > 4)
        {
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }
    }
}
