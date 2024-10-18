using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class EnemyAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack()
    {

    }

    
    protected virtual void SetMove(Vector3 position)
    {
        agent.destination = position;
    }

    protected bool dashing;
    
    protected Vector3 dashStartPosiiton;
    protected Vector3 dashEndPosiiton;
    private float dashTimer = 0;

    protected float dashSpeed;
    protected float dashDistance;

    protected virtual void Dash(Vector3 dashStartPosiiton, Vector3 dashEndPosiiton)
    {   
        dashTimer += Time.deltaTime;
        if (dashTimer < dashSpeed)
        {
            Vector3 newPos = Vector3.Lerp(dashStartPosiiton, dashEndPosiiton, dashTimer / dashSpeed);
            transform.position = newPos;
            dashing = true;
        }
        else
        {
            dashTimer = 0;
            dashing = false;
        }

    }

    protected virtual Vector3 GetRandomPointOnRadius(float radius)
    {
        Vector2 rand = Random.insideUnitCircle;
        return new Vector3(rand.x, 0, rand.y) * radius;
    }

    protected virtual Vector3 GetClosestPointInRadius(Vector3 center, float radius)
    {
        Vector2 c = new Vector2 (center.x, center.z);
        Vector2 p = new Vector2(transform.position.x, transform.position.z);

        Vector2 v = p - c;
        Vector2 output2D = c + v.normalized * radius;

        return new Vector3(output2D.x, 0 ,output2D.y);
    }

    protected float moveTime;
    protected float stopTime;
    private float moveTimer = 0;
    private float stopTimer = 0;
    protected float maxStopTime;
    protected float minStopTime;
    protected float maxMoveTime;
    protected float minMoveTime;
    protected virtual void StopInterval()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer > moveTime && !agent.isStopped)
        {
            agent.isStopped = true;
            moveTime = Random.Range(minMoveTime, maxMoveTime);
            moveTimer = 0;
        }
        if (moveTimer > stopTime && agent.isStopped)
        {
            agent.isStopped = false;
            stopTime = Random.Range(minStopTime, maxStopTime);
            moveTimer = 0;
        }
    }
}
