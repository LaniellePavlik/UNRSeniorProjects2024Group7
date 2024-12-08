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

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag.Equals(damageTag))
        {
            collider.GetComponent<Entity>().TakeDamage(baseDamage);
        }
    }
}
