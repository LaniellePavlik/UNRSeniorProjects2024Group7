using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    public bool locked;
    public Room room;

    // Start is called before the first frame update
    void Start()
    {
        PlayerMgr.inst.interactables.Add(this);
        room = gameObject.GetComponentInParent<Room>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        base.Interact();
        GameMgr.inst.NextRoom();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !locked)
        {
            interactable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactable = false;
        }
    }
}
