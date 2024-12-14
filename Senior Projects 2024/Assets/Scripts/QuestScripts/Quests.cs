using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64

public class Quests
{
    //Quest Info
    public PatronRequests info;
    public QuestState state;
    private int currentQuestStepIndex;
    private QuestStepState[] questStepStates;

    //Set a quest up from its scriptable object
    public Quests(PatronRequests questInfo)
    {
        this.info = questInfo;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;
        this.questStepStates = new QuestStepState[info.questStepPrefabs.Length];
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    //Hopefully restore saved data (Still needs work)
    public Quests(PatronRequests questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.info = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;

        //Check if data is out of sync
        if (this.questStepStates.Length != this.info.questStepPrefabs.Length)
        {
            Debug.LogWarning("prefab and state out of sync" + this.info.id);
        }
    }

    //Move to the next step by incrementing the index of steps
    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    //Check there is a step to go to next
    public bool currentStepExists()
    {
        return (currentQuestStepIndex < info.questStepPrefabs.Length);
    }

    //Init the prefab for quest steps that I step up indivdually for each quest
    public void InstantiateCurrentQuestStep(Transform parentTransform)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
            QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
                .GetComponent<QuestStep>();
            questStep.InitializeQuestStep(info.id, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
        }
    }

    //Get the current quest step prefab before inting it
    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if(currentStepExists())
        {
            questStepPrefab = info.questStepPrefabs[currentQuestStepIndex];
        }
        else
        {
            Debug.LogWarning("Index out of range");
        }
        return questStepPrefab;
    }

    //should theoretically save quest step data (Still needs work)
    public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
    {
        if (stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex].state = questStepState.state;
            questStepStates[stepIndex].status = questStepState.status;
        }
        else 
        {
            Debug.LogWarning("Step index out of range");
        }
    }

    //Pull the quest Data
     public QuestData GetQuestData()
    {
        return new QuestData(state, currentQuestStepIndex, questStepStates);
    }

    //Print out the status of the quest for debugging, quest log, and such
     public string GetFullStatusText()
    {
        string fullStatus = "";

        if (state == QuestState.REQUIREMENTS_NOT_MET)
        {
            fullStatus = "Requirements are not yet met to start this quest.";
        }
        else if (state == QuestState.CAN_START)
        {
            fullStatus = "This quest can be started!";
        }
        else 
        {
            //This puts strikethroughs
            for (int i = 0; i < currentQuestStepIndex; i++)
            {
                fullStatus += "<s>" + questStepStates[i].status + "</s>\n";
            }
            //Display current step
            if (currentStepExists())
            {
                fullStatus += questStepStates[currentQuestStepIndex].status;
            }
            //When quest can be completed or turned in
            if (state == QuestState.CAN_FINISH)
            {
                fullStatus += "The quest is ready to be turned in.";
            }
            else if (state == QuestState.FINISHED)
            {
                fullStatus += "The quest has been completed!";
            }
        }

        return fullStatus;
    }
}
