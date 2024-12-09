using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private PatronRequests questInfoForPoint;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;

    GameObject talkCanvas;
    PanelMover dialogue;
    Dialogue script;
    private bool playerIsNear = false;
    private string questId;
    [HideInInspector] public QuestState currentQuestState;

    private QuestIcon questIcon;

    public List<string> entryLines;
    public List<string> endingLines;

    private void Awake()
    {
        questId = questInfoForPoint.id;
        questIcon = GetComponentInChildren<QuestIcon>();
        talkCanvas = GameObject.FindGameObjectWithTag("Panel");
        dialogue = talkCanvas.GetComponent<PanelMover>();
        script = talkCanvas.GetComponent<Dialogue>();
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

        if (currentQuestState != QuestState.CAN_START && currentQuestState != QuestState.CAN_FINISH)
        {
            // dialogue.isVisible = true;
            script.StartDialogue();
        }

        else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
        {
            // dialogue.isVisible = true;
            script.EndQuestDialogue(endingLines);
            GameEventsManager.instance.questEvents.FinishQuest(questId);
        }
        if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
        {
            // dialogue.isVisible = true;
            script.StartQuestDialogue(entryLines);
            GameEventsManager.instance.questEvents.StartQuest(questId);
        }
    }

    private void QuestStateChange(Quests quest)
    {
        //Only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
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
