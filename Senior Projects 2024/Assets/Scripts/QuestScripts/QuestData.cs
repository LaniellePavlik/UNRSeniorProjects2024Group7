using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64 

[System.Serializable]
public class QuestData
{
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;

    //Store the quests current state, what step it's on, and those steps states
    public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates)
    {
        this.state = state;
        this.questStepIndex = questStepIndex;
        this.questStepStates = questStepStates;
    }
}