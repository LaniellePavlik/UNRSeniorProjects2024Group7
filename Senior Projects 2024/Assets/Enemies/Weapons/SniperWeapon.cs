//Script: SniperWeapon.cs
//Contributor: Liam Francisco
//Summary: Handles the weapon for the "Sniper" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperWeapon : Weapon
{
    public GameObject projectile; // prefab of the snipe projectile
    public Transform spawn; // spawn location of projectiles
    public AudioSource attackSound; // sound made when weapon is shot

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            StartAttack();
    }

    //spawns projectile and sets dirction
    public override void StartAttack()
    {
        GameObject newProjectile = Instantiate(projectile, spawn.position, Quaternion.identity);
        newProjectile.GetComponent<Projectile>().direction = transform.up;
        AudioMgr.Instance.PlaySFX("Ranged Attack", attackSound);
    }
}
