using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn
//Original Reference: https://www.youtube.com/watch?v=UyTJLDGcT64 
//With modification to account for more that I added on top of it like expanded event sysems
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set;}

    // public PatronRequests patronEvents;
    public QuestEvents questEvents;
    public InputEvents inputEvents;
    public MiscEvents miscEvents;
    public PlayerEvents playerEvents;
    
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
        playerEvents = new PlayerEvents();
    }
}
