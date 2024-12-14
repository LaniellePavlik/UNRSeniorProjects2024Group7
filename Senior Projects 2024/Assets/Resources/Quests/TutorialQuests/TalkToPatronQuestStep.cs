using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

//Author: Fenn 
//Quest to talk to the patron and test the LLM feature
public class TalkToPatronQuestStep : QuestStep
{
   int patronsSpokenTo = 0;
   int speakingGoal = 1;
   bool GhostSpoken = false;


    private void Start()
    {
        UpdateState();
    }

   private void OnEnable()
    {
        GameEventsManager.instance.miscEvents.onPatronTalked += SpokenTo;    
    }

    private void OnDisable()
    {
        GameEventsManager.instance.miscEvents.onPatronTalked -= SpokenTo; 
    }

    //sSeems redundant to finish quest step twice but accounts for edge cases later
    private void SpokenTo()
    {
        if (patronsSpokenTo >= speakingGoal)
        {
            FinishQuestStep();
        }
        if (patronsSpokenTo < speakingGoal)
        {
            patronsSpokenTo++;
            UpdateState(); 
            FinishQuestStep();
        }
    }

    //Update quest log
    private void UpdateState()
    {
        string state = patronsSpokenTo.ToString();
        string status = "Spoken To " + patronsSpokenTo + " / " + speakingGoal;
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.patronsSpokenTo = System.Int32.Parse(state);
        UpdateState();
    }
}
