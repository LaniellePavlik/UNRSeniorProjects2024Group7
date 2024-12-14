//Script: AOE.cs
//Contributor: Liam Francisco
//Summary: Handles the AI for the "AOE" enemy type

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AOE : EnemyAI
{
    public Transform player; // player's position
    public float AOEMaxStopTime; // max amount of time AOE will stop while moving
    public float AOEMinStopTime; // min amount of time AOE will stop while moving
    public float AOEMaxMoveTime; // max amount of time AOE will walk while moving
    public float AOEMinMoveTime; // min amount of time AOE will walk while moving
    public Animator enemyAni; // animations for attacking, moving, etc.
    public float attackCooldown = 0; // tracks time in between attacks

    // initializes EnemyAI parameters
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
        enemyAni.SetBool("Idle", false);

    }



    Vector3 movePosition; // where the AOE is going to move
    void Update()
    {
        //updates move position if AOE makes it to it's move pos to random pos around player
        float dist = Vector3.Distance(transform.position, movePosition);
        if(dist < 1)
        {
            movePosition = player.position + GetRandomPointOnRadius(10);
        }
        SetMove(movePosition);

        //Causes enemy to stop moving occasionally
        StopInterval();
        

        //Attacks every 4 seconds
        attackCooldown += Time.deltaTime;

        if (attackCooldown > 4)
        {
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }
    }
}
