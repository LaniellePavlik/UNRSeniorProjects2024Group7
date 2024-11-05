using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set;}

    // public PatronRequests patronEvents;
    public QuestEvents questEvents;
    public InputEvents inputEvents;
    public MiscEvents miscEvents;
    
    //Initalize everything
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one event mgr in scene");
        }
        instance = this;

        // patronEvents = new PatronRequests();
        questEvents = new QuestEvents();
        inputEvents = new InputEvents();
        miscEvents = new MiscEvents();
    }
}
