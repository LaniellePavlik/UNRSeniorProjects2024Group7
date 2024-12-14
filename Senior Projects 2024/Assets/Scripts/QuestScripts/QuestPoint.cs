using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64 and modifications

//Require a trigger component so we have an interatable area
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

    //Enable/Disable gameevents
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

    //This function checks for the user pressing E and then checks if the assoicated quest can be started, finished, or is currently active/inactive
    //Then it calls the LLM or quest dialogue accordingly
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

    //Gameevent in which quest state can be updated
    private void QuestStateChange(Quests quest)
    {
        //Only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }
    }

    //Check player proximity to NPC's with triggers
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
