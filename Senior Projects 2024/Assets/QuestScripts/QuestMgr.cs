using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestMgr : MonoBehaviour
{
    private Dictionary<string, Quests> questMap;

    private void Awake()
    {
        questMap = CreateQuestMap();
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.onStartQuest += StartQuest;
        GameEventsManager.Instance.questEvents.onAdvanceQuest += AdvanceQuest;
        GameEventsManager.Instance.questEvents.onFinishQuest += FinishQuest;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.Instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.Instance.questEvents.onFinishQuest -= FinishQuest;
    }

    private void Start()
    {
        //Broadcast the inital state of all quests on startup
        foreach (Quests quest in questMap.Values)
        {
            GameEventsManager.Instance.questEvents.QuestStateChange(quest);
        }
    }
    private void StartQuest(string id)
    {
        Debug.Log("Start Quest: " + id);
    }

    private void AdvanceQuest(string id)
    {
        Debug.Log("Advance Quest: " + id);

    }

    private void FinishQuest(string id)
    {
        Debug.Log("finish Quest: " + id);

    }

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

    private Quests GetQuestByID(string id)
    {
        Quests quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("ID not found in the Quest Map" + id);
        }
        return quest;
    }
}
