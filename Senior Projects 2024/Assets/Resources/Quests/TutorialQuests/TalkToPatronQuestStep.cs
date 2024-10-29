using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TalkToPatronQuestStep : QuestStep
{
   int patronsSpokenTo = 0;
   int speakingGoal = 1;

   private void OnEnable()
    {
        //When we have a listener for enemy defeats put it here
    }

    private void OnDisable()
    {
        //When we have a listener for enemy defeats put it here
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
