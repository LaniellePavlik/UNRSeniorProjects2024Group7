using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class FastButLight : EnemyAI
{
    public Transform player;
    public Animator enemyAni;
    public AudioSource dashSound;
    public int minRange = 3;
    public int maxRange = 5;
    // Start is called before the first frame update
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

    Vector3 movePosition;
    bool tripleDashStarted;
    public float dashCoolDownTimer;
    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        if (!tripleDashStarted)
        {
            agent.enabled = true;
            movePosition = GetClosestPointInRadius(player.position, 5);
            SetMove(movePosition);
            dashCoolDownTimer += Time.deltaTime;

            if (dist < 6 && dashCoolDownTimer > 10)
                StartTripleDash();
                
        }
        else
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
