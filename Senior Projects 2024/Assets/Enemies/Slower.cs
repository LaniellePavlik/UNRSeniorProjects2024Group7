using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Slower : EnemyAI
{
    public Transform player;
    public float orbitRadius;
        public Animator enemyAni;

    // Start is called before the first frame update
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        player = PlayerMgr.inst.player.transform;
        enemyAni.SetBool("isrunning", true);

    }


    Vector3 movePosition;
    float angle;
    public float attackCooldown;
    // Update is called once per frame
    void Update()
    {
        movePosition.x = player.position.x + orbitRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        movePosition.z = player.position.z + orbitRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        SetMove(movePosition);
        angle += Time.deltaTime * 30;
        attackCooldown += Time.deltaTime;

        //transform.LookAt(new Vector3(player.position.x, transform.position.y, player.transform.position.z), Vector3.up);

        if (Vector3.Distance(transform.position, player.position) < orbitRadius+2 && attackCooldown > 5)
        {
            enemyAni.SetTrigger("attack");
            entity.weapons[0].StartAttack();
            attackCooldown = 0;
        }
    }
}
