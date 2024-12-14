using System;

//Author: Fenn
//Original Reference: https://www.youtube.com/watch?v=UyTJLDGcT64 
//With modification to account for more that I added on top of it like expanded event sysems
public class QuestEvents
{
    //Action to start the quest by using it's id
    public event Action<string> onStartQuest;
    public void StartQuest(string id)
    {
        if (onStartQuest != null)
        {
            onStartQuest(id);
        }
    }

    //Action to advance the quest forward to the next step or finish state
    public event Action<string> onAdvanceQuest;
    public void AdvanceQuest(string id)
    {
        if (onAdvanceQuest != null)
        {
            onAdvanceQuest(id);
        }
    }

    //Action to finish a given quest by id
    public event Action<string> onFinishQuest;
    public void FinishQuest(string id)
    {
        if (onFinishQuest != null)
        {
            onFinishQuest(id);
        }
    }

    //this starts the change of a quest's state
    public event Action<Quests> onQuestStateChange;
    public void QuestStateChange(Quests quest)
    {
        if (onQuestStateChange != null)
        {
            onQuestStateChange(quest);
        }
    }

    //this changes the state of the quest to finished, canstart, ect.
    public event Action<string, int, QuestStepState> onQuestStepStateChange;
    public void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        if (onQuestStepStateChange != null)
        {
            onQuestStepStateChange(id, stepIndex, questStepState);
        }
    }
}


