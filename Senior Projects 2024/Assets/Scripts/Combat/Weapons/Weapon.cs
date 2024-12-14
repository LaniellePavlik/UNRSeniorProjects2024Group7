//Script: Weapon.cs
//Contributor: Liam Francisco
//Summary: Base class for all weapon types
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float baseDamage; // weapon damage
    public string damageTag; // tag of entities it damages

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void StartAttack()
    {

    }
}
