using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TalkToPatronQuestStep : QuestStep
{
   int patronsSpokenTo = 0;
   int speakingGoal = 1;

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

    private void SpokenTo()
    {
        if (patronsSpokenTo < speakingGoal)
        {
            patronsSpokenTo++;
            UpdateState();
            
        }

        if (patronsSpokenTo >= speakingGoal)
        {
            FinishQuestStep();
        }
    }

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
