using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    public QuestDisplayPrefab CreateDisplayIfNotExists(Quests quest) 
    {
        QuestDisplayPrefab questPrefab = null;
        // only create the questDisplay if we haven't seen this quest id before
        if (!idToQuestMap.ContainsKey(quest.info.id))
        {
            if(quest.state != QuestState.REQUIREMENTS_NOT_MET && quest.state != QuestState.CAN_START)
            {
                print(quest.state);
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

    private QuestDisplayPrefab InstantiateDisplayPrefab(Quests quest)
    {
        // create the display
        QuestDisplayPrefab createdQuestPrefab = Instantiate(
            questPrefab,
            contentParent.transform).GetComponent<QuestDisplayPrefab>();
        // game object name in the scene
        createdQuestPrefab.gameObject.name = quest.info.id + "_display";
        // initialize and set up function for when the display is selected
        // RectTransform displayRectTransform = createdQuestPrefab.GetComponent<RectTransform>();
        createdQuestPrefab.UpdateDisplay(quest.info.displayName, quest.GetFullStatusText());
        print(quest.GetFullStatusText());
        // add to map to keep track of the new quests
        idToQuestMap[quest.info.id] = createdQuestPrefab;
        return createdQuestPrefab;
    }

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
