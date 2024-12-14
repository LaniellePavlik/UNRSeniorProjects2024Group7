//Script: BigGuyWeapon.cs
//Contributor: Liam Francisco
//Summary: Handles the weapon for the "BigGuy" enemy type
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGuyWeapon : Weapon
{
    public Animator playerAni;
    public AudioSource attackSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool attacking;
    // handles whether or not the enemy should be swinging towards the player.
    void Update()
    {
        if (attacking)
        {
            Swing();
        }
    }

    //sets up the initial conditions for an attack to take place.
    public override void StartAttack()
    {
        attacking = true;
        windUp = true;
        AudioMgr.Instance.PlaySFX("Sword Slash", attackSound);
        playerAni.SetTrigger("attack");
    }

    //uses a Lerp to rotate the weapon GameObject to simulate the enemy swinging a club.
    bool windUp;
    bool swinging;
    float timer;
    public float windUpTime;
    public float swingTime;
    void Swing()
    {
        if (windUp)
        {
            timer += Time.deltaTime;
            float yRotation = Mathf.Lerp(0, -70, timer / windUpTime);
            transform.localEulerAngles = new Vector3(0, yRotation, 0);  
            if(timer > windUpTime)
            {
                swinging = true;
                windUp = false;
                timer = 0;
            }
        }
        else
        {
            timer += Time.deltaTime;
            float yRotation = Mathf.Lerp(-70, 70, timer / swingTime);
            transform.localEulerAngles = new Vector3(0, yRotation, 0);
            if (timer > swingTime)
            {
                swinging = false;
                hitThisSwing = false;
                attacking = false;
                timer = 0;
                transform.localEulerAngles = Vector3.zero;
            }
        }
    }

    //handles the player taking damage and makes sure that the damage is only applied once per swing
    bool hitThisSwing;
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag.Equals(damageTag))
        {
            if (!hitThisSwing && swinging)
            {
                collider.GetComponent<Entity>().TakeDamage(baseDamage);
                hitThisSwing = true;
            }
        }
    }
}
