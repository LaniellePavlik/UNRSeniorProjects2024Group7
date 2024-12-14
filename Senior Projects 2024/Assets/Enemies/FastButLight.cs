//Script: FastButLight.cs
//Contributor: Liam Francisco
//Summary: Handles the AI for the "FastButLight" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class FastButLight : EnemyAI
{
    public Transform player; // player's position
    public Animator enemyAni; // animations for attacking, moving, etc.
    public AudioSource dashSound; // sound played when dashing
    public int minRange = 3; // min dash distance
    public int maxRange = 5; // max dash distance

    // initializes EnemyAI parameters
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        movePosition = Vector3.zero;
        tripleDashStarted = false;
        inBetweenDashes = false;
        dashSpeed = .1f;
        player = PlayerMgr.inst.player.transform;
        enemyAni.SetBool("isrunning", true);
    }

    Vector3 movePosition; // where the FastButLight is going to move
    bool tripleDashStarted; // tells whether or not the enemy is in its dash sequence
    public float dashCoolDownTimer; // tracks time in between dashes
 
    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        if (!tripleDashStarted) //not dashing
        {
            //updates move position to make the FastButLight move towards the player
            agent.enabled = true;
            movePosition = GetClosestPointInRadius(player.position, 5);
            SetMove(movePosition);
            dashCoolDownTimer += Time.deltaTime;

            //start dash if in range and haven't dashed in 10 seconds
            if (dist < 6 && dashCoolDownTimer > 10)
                StartTripleDash();
                
        }
        else // dashing
        {
            if (!inBetweenDashes) 
                Dash(dashStartPosiiton, dashEndPosiiton);
            else 
                dashCoolDownTimer += Time.deltaTime;

            if(dashCoolDownTimer > .2f)
            {
                inBetweenDashes = false;
                dashCoolDownTimer = 0;
            }
        }
    }
    
    //initializes variables for the dash
    void StartTripleDash()
    {
        dashCoolDownTimer = 0;
        tripleDashStarted = true;
        agent.enabled = false;
        dashStartPosiiton = transform.position;
        dashEndPosiiton = player.position + GetRandomPointOnRadius(3);
        dashEndPosiiton.y = dashStartPosiiton.y;
        enemyAni.SetTrigger("attack");
    }

    // keeps track of how many dashes have gone and updates enemy position
    int dashCounter = 0;
    bool inBetweenDashes;
    protected override void Dash(Vector3 dashStartPosiiton, Vector3 dashEndPosiiton)
    {

        base.Dash(dashStartPosiiton, dashEndPosiiton);
        if (!dashing)
        {
            AudioMgr.Instance.PlaySFX("Dash", dashSound);
            dashCounter++;
            this.dashStartPosiiton = transform.position;
            this.dashEndPosiiton = transform.position + (player.position - transform.position).normalized * Random.Range(minRange,maxRange);
            this.dashEndPosiiton.y = this.dashStartPosiiton.y;
            inBetweenDashes = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.transform.position.z), Vector3.up);
            enemyAni.SetTrigger("attack");
        }
        if (dashCounter > 3)
        {
            dashCounter = 0;
            dashCoolDownTimer = 0;
            inBetweenDashes = false;
            tripleDashStarted = false;
        }
    }
}
