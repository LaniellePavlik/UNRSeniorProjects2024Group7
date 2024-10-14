using System.Collections;
using System.Collections.Generic;
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
        }

        if (patronsSpokenTo >= speakingGoal)
        {
            FinishQuestStep();
        }
    }
}
