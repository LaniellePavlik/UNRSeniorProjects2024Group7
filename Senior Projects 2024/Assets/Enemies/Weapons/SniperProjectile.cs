//Script: SniperProjectile.cs
//Contributor: Liam Francisco
//Summary: Handles the projectile for the "Sniper" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperProjectile : Projectile
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // updates projectile physics
    void Update()
    {
        UpdatePosition();
    }

    // handles when the projectile hits something
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Weapon"))
        {
            if(other.gameObject.tag.Equals(damageTag))
                other.GetComponent<Entity>().TakeDamage(baseDamage);
            Destroy(gameObject);
        }
    }
}
