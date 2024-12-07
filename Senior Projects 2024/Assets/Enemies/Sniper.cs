using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sniper : EnemyAI
{
    public Transform player;
    public float minRange;
    public float maxRange;
    public Animator enemyAni;
    // Start is called before the first frame update
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        player = PlayerMgr.inst.player.transform;
        enemyAni.SetBool("Idle", false);
    }


    Vector3 movePosition;
    float attackCooldown;
    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, movePosition);
        if(dist < minRange || dist > maxRange)
        {
            movePosition = GetClosestPointInRadius(player.transform.position, (maxRange+minRange)/2);
        }
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.transform.position.z), Vector3.up);
        SetMove(movePosition);

        attackCooldown += Time.deltaTime;

        if (attackCooldown > 2)
        {
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }
    }
}
