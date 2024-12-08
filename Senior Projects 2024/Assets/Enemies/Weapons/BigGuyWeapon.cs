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
    // Update is called once per frame
    void Update()
    {
        if (attacking)
        {
            Swing();
        }
    }

    public override void StartAttack()
    {
        attacking = true;
        windUp = true;
        AudioMgr.Instance.PlaySFX("Sword Slash", attackSound);
        playerAni.SetTrigger("attack");
    }

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
