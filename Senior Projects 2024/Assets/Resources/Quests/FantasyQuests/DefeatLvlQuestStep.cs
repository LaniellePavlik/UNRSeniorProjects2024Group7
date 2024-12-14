using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author: Fenn
//The logic behind where the defeat level quest will go, aka winning the first level
//The logic has yet to be fully implemented but is set up

public class DefeatLvlQuestStep : QuestStep
{
bool levelCompleted = false;
int currentState = 0;
int goalState = 1;
   private void Start()
    {
        UpdateState();
    }

   private void OnEnable()
    {
        GameEventsManager.instance.miscEvents.onPatronTalked += Defeated;    
    }

    private void OnDisable()
    {
        GameEventsManager.instance.miscEvents.onPatronTalked -= Defeated;
    }

    private void Defeated()
    {
        if (levelCompleted == true)
        {
            FinishQuestStep();
        }
    }

    //Check for the level being done
    private void UpdateState()
    {
        string state = currentState.ToString();
        string status = "Lvl Done: " + currentState + " / " + goalState;
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.currentState = System.Int32.Parse(state);
        UpdateState();
    }
}
