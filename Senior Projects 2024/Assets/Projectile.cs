using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 direction;
    public float speed;
    public float acceleration;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    float timeAlive = 0;
    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
        speed += acceleration * Time.deltaTime;

        if(timeAlive > 6)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.tag.Equals("Weapon"))
        {
            Debug.Log("hit");
            Destroy(gameObject);
        }
    }
}
