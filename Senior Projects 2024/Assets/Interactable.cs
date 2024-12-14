//Script: Interactable.cs
//Contributor: Liam Francisco
//Summary: Base class for all interactable objects
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool interactable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Interact()
    {
        Debug.Log("interacted with " + gameObject.name);
    }
}
