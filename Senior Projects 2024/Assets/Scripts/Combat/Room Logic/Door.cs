//Script: Door.cs
//Contributor: Liam Francisco
//Summary: Class for any door objects in a combat world
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    public bool locked;
    public Room room;

    // adds this door to the PlayerMgr’s list of interactables so the player can reference it in its Interact method in PlayerController
    void Start()
    {
        PlayerMgr.inst.interactables.Add(this);
        room = gameObject.GetComponentInParent<Room>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //loads the next room
    public override void Interact()
    {
        base.Interact();
        GameMgr.inst.NextRoom();
    }

    //detects when the player is in the interacting range of the door.
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !locked)
        {
            interactable = true;
        }
    }

    //detects when the player leaves the interacting range of the door.
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactable = false;
        }
    }
}
