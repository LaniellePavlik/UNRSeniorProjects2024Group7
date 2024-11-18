using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{

    public int sceneIndex;
    SceneSwitch sceneSwitch;

    public void pressedButton()
    {
        sceneSwitch.currentScene = sceneIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        sceneSwitch = GameObject.FindObjectOfType<SceneSwitch>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
