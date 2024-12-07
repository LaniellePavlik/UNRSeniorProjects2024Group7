using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class SlowerWeapon : Weapon
{

    bool attacking;
    Slower entity;
    // public Animator enemyAni;

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponentInParent<Slower>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attacking)
        {
            Stab();
        }
    }

    public override void StartAttack()
    {
        attacking = true;
        stabbing = true;
    }

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

    bool hitThisStab;
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag.Equals(damageTag))
        {
            if (!hitThisStab && stabbing)
            {
                collider.GetComponent<Entity>().TakeDamage(baseDamage);
                StartCoroutine(PlayerMgr.inst.SlowDown(0.5f, 5));
                hitThisStab = true;
            }
        }
    }
}


