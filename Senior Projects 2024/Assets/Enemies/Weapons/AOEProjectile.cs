//Script: AOEProjectile.cs
//Contributor: Liam Francisco
//Summary: Handles the projectile for the "AOE" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEProjectile : Projectile
{
    public Transform player; // player's position
    public GameObject debugSphere; // shows explosion radius
    void Start()
    {
        baseDamage = 5;
    }

    // handles projectile physics
    void Update()
    {
        Vector3 velocity = direction * speed;
        velocity.y -= 9.81f * Time.deltaTime;
        speed = velocity.magnitude;
        direction = velocity.normalized;
        UpdatePosition();
    }

    // handles projectile explosion
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Weapon") && !other.gameObject.tag.Equals("Enemy"))
        {
            GameObject debug = Instantiate(debugSphere, transform.position, Quaternion.identity);
            debug.transform.localScale = 6f*Vector3.one;
            if(Vector3.Distance(player.position, transform.position) < 3)
            {
                Debug.Log(Vector3.Distance(player.position, transform.position));
                player.GetComponent<Entity>().TakeDamage(baseDamage);
            }
            Destroy(gameObject);
        }
    }
}
