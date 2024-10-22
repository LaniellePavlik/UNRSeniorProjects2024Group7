using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private PatronRequests questInfoForPoint;

    private bool playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;

    private void Awake()
    {
        questId = questInfoForPoint.id;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
        GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
        GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
    }

    private void SubmitPressed()
    {
        if (!playerIsNear)
        {
            return;
        }

        GameEventsManager.instance.questEvents.StartQuest(questId);
        GameEventsManager.instance.questEvents.AdvanceQuest(questId);
        GameEventsManager.instance.questEvents.FinishQuest(questId);
    }

    private void QuestStateChange(Quests quest)
    {
        //Only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            Debug.Log("Quest with id: " + questId + "Update to state: " + currentQuestState);
        }
    }

    private void OnTriggerEnter(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider otherColldier)
    {
        if (otherColldier.tag == "Player")
        {
            playerIsNear = false;
        }
    }
}
