//Script: Projectile.cs
//Contributor: Liam Francisco
//Summary: Base class for all projectiles
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 direction;
    public float speed;
    public float acceleration;
    public float lifetime;
    public float baseDamage;
    public string damageTag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected float timeAlive = 0;
    // Update is called once per frame
    void Update()
    {

    }

    //handles where the projectiles next location should be based on the direction it’s traveling and what its speed and acceleration are
    protected virtual void UpdatePosition()
    {
        timeAlive += Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
        speed += acceleration * Time.deltaTime;

        if (timeAlive > lifetime)
        {
            Destroy(gameObject);
        }
    }

    // deals damage
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Weapon"))
        {
            other.GetComponent<Entity>().TakeDamage(baseDamage);
            Destroy(gameObject);
        }
    }
}
