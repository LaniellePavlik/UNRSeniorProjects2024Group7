using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//Author: Fenn
//Creates the HUD questlog 
//Reference: https://www.youtube.com/watch?v=ZYVED_aLHj0&t=503s

public class QuestLogNormalUI : MonoBehaviour
{
    public GameObject questPrefab;
    public GameObject contentParent;
    public RectTransform rectTransform;
    private int DisplayedQuestCount = 0;
    private Dictionary<string, QuestDisplayPrefab> idToQuestMap = new Dictionary<string, QuestDisplayPrefab>();

    void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Create a display for the quest logs on HUD
    public QuestDisplayPrefab CreateDisplayIfNotExists(Quests quest) 
    {
        QuestDisplayPrefab questPrefab = null;
        // only create the questDisplay if we haven't seen this quest id before
        if (!idToQuestMap.ContainsKey(quest.info.id))
        {
            if(quest.state != QuestState.REQUIREMENTS_NOT_MET && quest.state != QuestState.CAN_START)
            {
                questPrefab = InstantiateDisplayPrefab(quest);
            }
        }
        else 
        {
            questPrefab = idToQuestMap[quest.info.id];
        }
        updateLayout();
        return questPrefab;
    }

    public void updateLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    //Instantiate all the quest prefabs for the HUD log
    private QuestDisplayPrefab InstantiateDisplayPrefab(Quests quest)
    {
        QuestDisplayPrefab createdQuestPrefab = Instantiate(
            questPrefab,
            contentParent.transform).GetComponent<QuestDisplayPrefab>();
        createdQuestPrefab.gameObject.name = quest.info.id + "_display";
        // RectTransform displayRectTransform = createdQuestPrefab.GetComponent<RectTransform>();
        createdQuestPrefab.UpdateDisplay(quest.info.displayName, quest.GetFullStatusText());
        print(quest.GetFullStatusText());
        //Add to map to keep track of the new quests
        idToQuestMap[quest.info.id] = createdQuestPrefab;
        return createdQuestPrefab;
    }

    //Change quest state when it should change
    public void QuestStateChange(Quests quest)
    {
        QuestDisplayPrefab questLogPrefab = CreateDisplayIfNotExists(quest);
        foreach(KeyValuePair<string, QuestDisplayPrefab> entry in idToQuestMap)
        {
            if(entry.Key == quest.info.id)
            {
                entry.Value.UpdateDisplay(quest.info.displayName, quest.GetFullStatusText());
            }
        }
        updateLayout();
    }
}
