using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperWeapon : Weapon
{
    public GameObject projectile;
    public Transform spawn;
    public AudioSource attackSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            StartAttack();
    }

    public override void StartAttack()
    {
        GameObject newProjectile = Instantiate(projectile, spawn.position, Quaternion.identity);
        newProjectile.GetComponent<Projectile>().direction = transform.up;
        AudioMgr.Instance.PlaySFX("Ranged Attack", attackSound);
    }
}
