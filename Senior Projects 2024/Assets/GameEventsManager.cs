using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set;}

    public PatronRequests patronEvents;
    public QuestEvents questEvents;

    public InputEvents inputEvents;
    
    //Initalize everything
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one event mgr in scene");
        }
        Instance = this;

        patronEvents = new PatronRequests();
        questEvents = new QuestEvents();
        inputEvents = new InputEvents();
    }
}
