//Script: Sniper.cs
//Contributor: Liam Francisco
//Summary: Handles the AI for the "Sniper" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sniper : EnemyAI
{
    public Transform player; // player's position
    public float minRange; // min distance between Sniper and player
    public float maxRange; // max distance between Sniper and player
    public Animator enemyAni; // animations for attacking, moving, etc.

    // initializes EnemyAI parameters
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        player = PlayerMgr.inst.player.transform;
        enemyAni.SetBool("Idle", false);
    }


    Vector3 movePosition; // where the Slower is going to move
    float attackCooldown; // tracks time in between attacks

    void Update()
    {
        //updates move position to make the Sniper be in between minRange and maxRange units away from the player
        float dist = Vector3.Distance(player.position, movePosition);
        if(dist < minRange || dist > maxRange)
        {
            movePosition = GetClosestPointInRadius(player.transform.position, (maxRange+minRange)/2);
        }
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.transform.position.z), Vector3.up);
        SetMove(movePosition);

        //Attacks every 2 seconds
        attackCooldown += Time.deltaTime;

        if (attackCooldown > 2)
        {
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }

        //Handles walk vs idle animations
        if (agent.isStopped)
            enemyAni.SetBool("Idle", true);
        else
            enemyAni.SetBool("Idle", false);
    }
}
