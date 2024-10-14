using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
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
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
        GameEventsManager.Instance.inputEvents.onSubmitPressed += SubmitPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
        GameEventsManager.Instance.inputEvents.onSubmitPressed -= SubmitPressed;
    }

    private void SubmitPressed()
    {
        if (!playerIsNear)
        {
            return;
        }

        GameEventsManager.Instance.questEvents.StartQuest(questId);
        GameEventsManager.Instance.questEvents.AdvanceQuest(questId);
        GameEventsManager.Instance.questEvents.FinishQuest(questId);
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

    private void OnTriggerEnter2D(Collider2D otherColldier)
    {
        if (otherColldier.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D otherColldier)
    {
        if (otherColldier.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}
