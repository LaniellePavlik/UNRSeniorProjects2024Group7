using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Author: Fenn
//Loads scenes depending on the situation
public class SceneSwitch : MonoBehaviour
{
    [HideInInspector]
    public int currentScene = 0;

    public void LoadScene()
    {
        StartCoroutine(LoadYourAsyncScene());
    }

    //Basic Load
   IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentScene);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    //Sets the current scene to Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentScene = 0;
        LoadScene();
    }

    //Quits the game
    public void QuitGame()
    {
        Application.Quit();
    }
}