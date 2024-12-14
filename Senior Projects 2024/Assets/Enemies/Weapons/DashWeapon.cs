//Script: SlowerWeapon.cs
//Contributor: Liam Francisco
//Summary: Handles the weapon for the "FastButLight" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashWeapon : Weapon
{
    // Start is called before the first frame update
    public FastButLight enemy;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // damages player if hit by dash
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag.Equals(damageTag))
        {
            collider.GetComponent<Entity>().TakeDamage(baseDamage);
        }
    }
}
