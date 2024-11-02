using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMgr : MonoBehaviour
{
    public float baseSpeed;
    public float speed;
    public float baseHealth;

    public Entity player;

    public static PlayerMgr inst;
    
    // Start is called before the first frame update
    void Start()
    {
        inst = this;
        speed = baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        player.speed = speed;
    }

    public IEnumerator SlowDown(float multiplier, float timeSlowed)
    {
        speed *= multiplier;
        yield return new WaitForSeconds(timeSlowed);
        speed /= multiplier;
    }
}
