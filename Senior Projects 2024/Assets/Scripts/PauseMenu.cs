using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
