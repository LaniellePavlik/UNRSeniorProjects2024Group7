using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BookSwitch : MonoBehaviour
{
    private bool playerIsNear = false;

    public SceneSwitch sceneSwitch;


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
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = false;
        }
    }
}
