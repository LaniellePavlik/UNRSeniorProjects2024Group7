using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64

public abstract class QuestStep : MonoBehaviour
{
    private bool isFinished = false;
    private string questId;
    private int stepIndex;

    //Init the quest step
    public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;
        if (questStepState != null && questStepState != "")
        {
            SetQuestStepState(questStepState);
        }
    }

    //Quest step is completed
    protected void FinishQuestStep()
    {
        if (!isFinished)
        {
            isFinished = true;
            GameEventsManager.instance.questEvents.AdvanceQuest(questId);
            Destroy(this.gameObject);
        }
    }

    //Quest step has a new state
    protected void ChangeState(string newState, string newStatus)
    {
        GameEventsManager.instance.questEvents.QuestStepStateChange(
            questId, 
            stepIndex, 
            new QuestStepState(newState, newStatus)
        );
    }

    protected abstract void SetQuestStepState(string state);
}
