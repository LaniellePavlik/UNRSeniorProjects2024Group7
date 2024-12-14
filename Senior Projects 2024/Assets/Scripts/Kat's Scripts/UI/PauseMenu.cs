using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//just a script to create a pause menu that pops us when the player presses escape. 
//eventually added a physical button to pause but escape still works
//it also pauses player movement bc thats what youre supposed to do when youre paused
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public SceneSwitch sceneSwitch;
    public static bool isPaused;
    public PanelMover UIpanel;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIpanel.isVisible == true)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        UIpanel.isVisible = true;
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
    }
    public void ResumeGame()
    {
        UIpanel.isVisible = false;
        GameEventsManager.instance.playerEvents.EnablePlayerMovement();
    }
}
