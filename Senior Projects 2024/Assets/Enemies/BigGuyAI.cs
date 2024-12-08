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
    public Animator enemyAni;


    public float attackCooldown = 0;
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

        player = PlayerMgr.inst.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        SetMove(player.position);
        StopInterval();

        attackCooldown += Time.deltaTime;

        Debug.Log(Vector3.Distance(transform.position, player.position));

        if (Vector3.Distance(transform.position, player.position) < 2 && attackCooldown > 5)
        {
            Debug.Log("started");
            entity.weapons[0].StartAttack();
            enemyAni.SetTrigger("attack");
            attackCooldown = 0;
        }
    }
}
