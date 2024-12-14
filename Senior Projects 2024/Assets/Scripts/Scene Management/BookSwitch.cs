using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Author: Fenn
public class BookSwitch : MonoBehaviour
{
    private bool playerIsNear = false; //This just checks for if the player is close to activate the trigger

    public SceneSwitch sceneSwitch; //Reference to switch scenes


    //Call upon the game event manager to add my events
    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
    }

    void Start()
    {
 
    }

    void Update()
    {
    }

    //I forgot to get rid of my debugs and I'm scared to break something so pretend
    //Like they don't exist thank you :))
    private void SubmitPressed()
    {
        if (!playerIsNear)
        {
            
            print("Here2");
            return;
        }

        print("Here");
        sceneSwitch.currentScene = 2;
        sceneSwitch.LoadScene();
        
    }

    // Checks for the player entering the trigger area in which you can interact with the book
    private void OnTriggerEnter(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = true;
        }
    }

    // Checks for the player leaving the trigger area in which you can interact with the book 
    // (and makes it so they can't interact with it outside a certain range)
    private void OnTriggerExit(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = false;
        }
    }
}
