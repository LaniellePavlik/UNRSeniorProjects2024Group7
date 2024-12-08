using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BookSwitch : MonoBehaviour
{
    private bool playerIsNear = false;

    public SceneSwitch sceneSwitch;
    public GameObject book;
    public QuestPoint questPoint;

    void Start()
    {
        book.SetActive(false);
    }

    void Update()
    {
        if(questPoint.currentQuestState == QuestState.FINISHED)
        {
            book.SetActive(true);
        }
    }

    private void SubmitPressed()
    {
        if (!playerIsNear)
        {
            return;
        }

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
