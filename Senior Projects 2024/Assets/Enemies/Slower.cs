using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Slower : EnemyAI
{
    public Transform player;
    public float orbitRadius;
    // Start is called before the first frame update
    void Start()
    {
        movePosition = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
    }


    Vector3 movePosition;
    float angle;
    // Update is called once per frame
    void Update()
    {
        movePosition.x = player.position.x + orbitRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        movePosition.z = player.position.z + orbitRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        SetMove(movePosition);
        angle += Time.deltaTime * 30;
    }
}
