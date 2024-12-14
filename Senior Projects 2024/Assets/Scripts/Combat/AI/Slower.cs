//Script: Slower.cs
//Contributor: Liam Francisco
//Summary: Handles the AI for the "Slower" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Slower : EnemyAI
{
    public Transform player; // player's position
    public float orbitRadius; // radius at which the enemy will circle the player
    public Animator enemyAni; // animations for attacking, moving, etc.

    // initializes EnemyAI parameters
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        player = PlayerMgr.inst.player.transform;
        enemyAni.SetBool("isrunning", true);

    }


    Vector3 movePosition; // where the Slower is going to move
    float angle; // used in calculating where around the player the Slower is
    public float attackCooldown; // tracks time in between attacks
    void Update()
    {
        //updates move position to make the Slower circle the player
        movePosition.x = player.position.x + orbitRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        movePosition.z = player.position.z + orbitRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        SetMove(movePosition);
        angle += Time.deltaTime * 30;

        //Attacks if 2+orbitRadius units away from the player and if its been more than 5 seconds in between attacks
        attackCooldown += Time.deltaTime;

        if (Vector3.Distance(transform.position, player.position) < orbitRadius+2 && attackCooldown > 5)
        {
            enemyAni.SetTrigger("attack");
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }
    }
}
