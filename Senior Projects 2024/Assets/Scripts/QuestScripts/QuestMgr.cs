using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64 and modifications

public class QuestMgr : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private bool loadQuestState = true;
    private Dictionary<string, Quests> questMap;

    //Start with creating a dictonary of id's and quests
    private void Awake()
    {
        questMap = CreateQuestMap();
    }

    //Initalize all my gameevenets related to quests, such as moving through states
    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest += AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;

        GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest -= FinishQuest;

        GameEventsManager.instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;

    }

    private void Start()
    {
        //I kept breaking the UI so it got modified
        Screen.SetResolution(1920, 1080, true);
        foreach (Quests quest in questMap.Values)
        {
             //Init loaded quest steps
            if (quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }
    }

    //The gameEvent in which the quest state will update based off the parameters ex: canstart, finished
     private void ChangeQuestState(string id, QuestState state)
    {
        Quests quest = GetQuestByID(id);
        quest.state = state;
        GameEventsManager.instance.questEvents.QuestStateChange(quest);
    }

    //Make sure the requirements for the quest are met
    private bool CheckRequirementsMet(Quests quest)
    {
        bool meetsRequirements = true;

        //I don't have a lvl thing yet but this is there incase we add one

        // check player level requirements
        // if (currentPlayerLevel < quest.info.levelRequirement)
        // {
        //     meetsRequirements = false;
        // }

        //Check quest prerecs
        foreach (PatronRequests prerequisiteQuestInfo in quest.info.questPrerecs)
        {
            if (GetQuestByID(prerequisiteQuestInfo.id).state != QuestState.FINISHED)
            {
                meetsRequirements = false;
            }
        }

        return meetsRequirements;
    }

     private void Update()
    {
        //Loop thru quests
        foreach (Quests quest in questMap.Values)
        {
            //If requirements are met, go to the quest change can start state
            if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
            {
                ChangeQuestState(quest.info.id, QuestState.CAN_START);
            }
        }
    }

    //Gameevent where we actually start the quest and boot up the first step!
    private void StartQuest(string id)
    {
        Quests quest = GetQuestByID(id);
        quest.InstantiateCurrentQuestStep(this.transform);
        ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
    }

    //Go to the next step, if there is none the quest can advance
    private void AdvanceQuest(string id)
    {
        Quests quest = GetQuestByID(id);

        quest.MoveToNextStep();

        //Init next step
        if (quest.currentStepExists())
        {
            quest.InstantiateCurrentQuestStep(this.transform);
        }

        else
        {
            ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
        }
    }

    //Gameevent for when the quest is finished and rewards can be claimed
    private void FinishQuest(string id)
    {
        Quests quest = GetQuestByID(id);
        ClaimRewards(quest);
        ChangeQuestState(quest.info.id, QuestState.FINISHED);
    }

    private void ClaimRewards(Quests quest)
    {
        //I have nothing implemented for rewards but itll happen eventually

        // GameEventsManager.instance.goldEvents.GoldGained(quest.info.goldReward);
    }

    //Game event action where we change the quest state
    private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
    {
        Quests quest = GetQuestByID(id);
        quest.StoreQuestStepState(questStepState, stepIndex);
        ChangeQuestState(id, quest.state);
    }

    //Creates the map of id's to quests
    private Dictionary<string, Quests> CreateQuestMap()
    {
        PatronRequests[] allQuests = Resources.LoadAll<PatronRequests>("Quests");
        Dictionary<string, Quests> idToQuestMap = new Dictionary<string, Quests>();
        foreach (PatronRequests questInfo in allQuests)
        {
            if(idToQuestMap.ContainsKey(questInfo.id))
            {
                Debug.LogWarning("Dupe ID found" + questInfo.id);
            }
            idToQuestMap.Add(questInfo.id, new Quests(questInfo));
        }
        return idToQuestMap;
    }

    //Retrieves the quest in the map
    private Quests GetQuestByID(string id)
    {
        Quests quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("ID not found in the Quest Map" + id);
        }
        return quest;
    }

    //Theoretically saves quests if we quit
     private void OnApplicationQuit()
    {
        foreach (Quests quest in questMap.Values)
        {
            SaveQuest(quest);
        }
    }

    //Save the queststate (Still needs work)
    private void SaveQuest(Quests quest)
    {
        try 
        {
            QuestData questData = quest.GetQuestData();
            //Save everything to a json
            string serializedData = JsonUtility.ToJson(questData);
            PlayerPrefs.SetString(quest.info.id, serializedData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save quest with id " + quest.info.id + ": " + e);
        }
    }

    //Load the quest on boot (Still needs work)
    private Quests LoadQuest(PatronRequests questInfo)
    {
        Quests quest = null;
        try 
        {
            //Load quest from data
            if (PlayerPrefs.HasKey(questInfo.id) && loadQuestState)
            {
                string serializedData = PlayerPrefs.GetString(questInfo.id);
                QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                quest = new Quests(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
            }
            //Or add a new quest
            else 
            {
                quest = new Quests(questInfo);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
        }
        return quest;
    }
}
