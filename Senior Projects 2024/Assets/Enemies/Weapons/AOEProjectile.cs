using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEProjectile : Projectile
{
    // Start is called before the first frame update
    public Transform player;
    public GameObject debugSphere;
    void Start()
    {
        baseDamage = 5;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = direction * speed;
        velocity.y -= 9.81f * Time.deltaTime;
        speed = velocity.magnitude;
        direction = velocity.normalized;
        UpdatePosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Weapon") && !other.gameObject.tag.Equals("Enemy"))
        {
            GameObject debug = Instantiate(debugSphere, transform.position, Quaternion.identity);
            debug.transform.localScale = 6f*Vector3.one;
            if(Vector3.Distance(player.position, transform.position) < 3)
            {
                Debug.Log(Vector3.Distance(player.position, transform.position));
                player.GetComponent<Entity>().TakeDamage(baseDamage);
            }
            Destroy(gameObject);
        }
    }
}
