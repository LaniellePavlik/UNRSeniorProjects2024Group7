using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sniper : EnemyAI
{
    public Transform player;
    public float minRange;
    public float maxRange;
    // Start is called before the first frame update
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
    }


    Vector3 movePosition;
    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, movePosition);
        if(dist < minRange || dist > maxRange)
        {
            movePosition = GetClosestPointInRadius(player.transform.position, (maxRange+minRange)/2);
        }
        Debug.Log(dist);
        SetMove(movePosition);
    }

    /*
    void GetNewMovePos()
    {
        float dist = Vector3.Distance(player.position, movePosition);
        while (dist < minRange || dist > maxRange)
        {
            movePosition = player.position + GetRandomPointOnRadius(10);
            dist = Vector3.Distance(player.position, movePosition);
        }
    }
    */
}
