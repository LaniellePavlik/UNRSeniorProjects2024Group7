//Script: BigGuyAI.cs
//Contributor: Liam Francisco
//Summary: Handles the AI for the "BigGuy" enemy type

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BigGuyAI : EnemyAI
{
    public Transform player; // player's position
    public float BGMaxStopTime; // max amount of time BigGuy will stop while moving
    public float BGMinStopTime; // min amount of time BigGuy will stop while moving
    public float BGMaxMoveTime; // max amount of time BigGuy will walk while moving
    public float BGMinMoveTime; // min amount of time BigGuy will walk while moving
    public Animator enemyAni; // animations for attacking, moving, etc.
    public float attackCooldown = 0; // tracks time in between attacks

    // initializes EnemyAI parameters
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        maxStopTime = BGMaxStopTime;
        minStopTime = BGMinStopTime;
        maxMoveTime = BGMaxMoveTime;
        minMoveTime = BGMinMoveTime;

        moveTime = Random.Range(minMoveTime, maxMoveTime);
        stopTime = Random.Range(minStopTime, maxStopTime);

        player = PlayerMgr.inst.player.transform;
    }

    void Update()
    {
        // tells BigGuy to walk towards the player
        SetMove(player.position);

        //Causes enemy to stop moving occasionally
        StopInterval();


        //Attacks if 2 units away from the player and if its been more than 5 seconds in between attacks
        attackCooldown += Time.deltaTime;

        if (Vector3.Distance(transform.position, player.position) < 2 && attackCooldown > 5)
        {
            Debug.Log("started");
            entity.weapons[0].StartAttack();
            enemyAni.SetTrigger("attack");
            attackCooldown = 0;
        }
    }
}
