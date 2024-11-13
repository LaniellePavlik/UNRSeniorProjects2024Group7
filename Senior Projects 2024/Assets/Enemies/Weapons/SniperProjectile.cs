using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperProjectile : Projectile
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

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
