//Script: SlowerWeapon.cs
//Contributor: Liam Francisco
//Summary: Handles the weapon for the "Slower" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowerWeapon : Weapon
{
    bool attacking; // determines whether weapon is mid swing
    Slower entity; // point to enemy entity
    public AudioSource attackSound; // sound when emeny swings
    // public Animator enemyAni;

    //finds the SlowerAI component associated with the weapon so its location can be used in attack calculations
    void Start()
    {
        entity = GetComponentInParent<Slower>();
    }

    // handles whether or not the enemy should be stabbing towards the player
    void Update()
    {
        if (attacking)
        {
            Stab();
        }
    }

    //sets up the initial conditions for an attack to take place.
    public override void StartAttack()
    {
        attacking = true;
        stabbing = true;
        AudioMgr.Instance.PlaySFX("Sword Slash", attackSound);
    }

    //handles the wind up and thrust animations of the enemy’s attack.
    bool stabbing;
    float timer;
    public float stabTime;
    public float windDownTime;

    void Stab()
    {
        if (stabbing)
        {
            // enemyAni.SetTrigger("attack");
            timer += Time.deltaTime;
            float zPosition = Mathf.Lerp(0, entity.orbitRadius, timer / stabTime);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zPosition);
            if (timer > stabTime)
            {
                stabbing = false;
                timer = 0;
            }
        }
        else
        {
            timer += Time.deltaTime;
            float zPosition = Mathf.Lerp(entity.orbitRadius, 0, timer / windDownTime);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zPosition);
            if (timer > windDownTime)
            {
                hitThisStab = false;
                attacking = false;
                timer = 0;
            }
        }
    }

    //applies both the damage and the slow effect to the player if they are hit by the weapon.
    bool hitThisStab;
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag.Equals(damageTag))
        {
            if (!hitThisStab && stabbing)
            {
                collider.GetComponent<Entity>().TakeDamage(baseDamage);
                //StartCoroutine(PlayerMgr.inst.SlowDown(0.5f, 5));
                hitThisStab = true;
            }
        }
    }
}


