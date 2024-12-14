using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64

//This file just gets the state and status of the quest step
[System.Serializable]
public class QuestStepState
{
    public string state;
    public string status;

    public QuestStepState(string state, string status)
    {
        this.state = state;
        this.status = status;
    }

    public QuestStepState()
    {
        this.state = "";
        this.status = "";
    }
}