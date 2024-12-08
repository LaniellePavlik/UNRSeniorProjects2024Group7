using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AOEWeapon : Weapon
{
    // Start is called before the first frame update
    public GameObject projectile;
    AOE entity;
    public AudioSource attackSound;

    void Start()
    {
        entity = GetComponentInParent<AOE>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void StartAttack()
    {
        GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
        AOEProjectile proj = newProjectile.GetComponent<AOEProjectile>();
        proj.player = entity.player;
        proj.direction = entity.player.position - transform.position;
        proj.direction.Normalize();
        Vector2 startPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 endPos = new Vector2(entity.player.position.x, entity.player.position.z);
        float dist = Vector2.Distance(startPos, endPos);
        float throwAngle = 45 * Mathf.Deg2Rad;
        proj.speed = Mathf.Sqrt(9.81f * dist /Mathf.Sin(2 * throwAngle));
        proj.direction.y = Mathf.Sin(throwAngle);
        proj.direction.Normalize();
        AudioMgr.Instance.PlaySFX("Ranged Attack", attackSound);
    }
}
