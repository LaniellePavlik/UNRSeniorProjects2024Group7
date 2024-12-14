using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
//Author: Fenn
//Reference: https://www.youtube.com/watch?v=ZYVED_aLHj0&t=503s with modifications
//This defines the quest log UI shown when pressing Q
public class QuestLogUI : MonoBehaviour
{
    [Header("Components")]
    public GameObject contentParent;
    public QuestLogScrollingList scrollingList;
    public TextMeshProUGUI questDisplayNameText;
    public TextMeshProUGUI questStatusText;
    public TextMeshProUGUI experienceRewardsText;
    public PanelMover UIpanel;

    private Button firstSelectedButton;

    //Enable gameevents
    void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onQuestLogTogglePressed += QuestLogTogglePressed;
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onQuestLogTogglePressed -= QuestLogTogglePressed;
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    //If Q is pressed either show or hide the log
    public void QuestLogTogglePressed()
    {
        if (UIpanel.isVisible == true)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
    }

    //If the log is visible, deactivate player movement
    public void ShowUI()
    {
        UIpanel.isVisible = true;
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        if (firstSelectedButton != null)
        {
            firstSelectedButton.Select();
        }
    }

    //If the log is disabled, activate player movement
    public void HideUI()
    {
        UIpanel.isVisible = false;
        GameEventsManager.instance.playerEvents.EnablePlayerMovement();
        EventSystem.current.SetSelectedGameObject(null);
    }

    //Update the quest log to reflect the quest states
    public void QuestStateChange(Quests quest)
    {
        //Add to the list
        QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () => {
            SetQuestLogInfo(quest);
        });

        if (firstSelectedButton == null)
        {
            firstSelectedButton = questLogButton.button;
        }

        //Set colors to different states!
        questLogButton.SetState(quest.state);
    }

    //Sets the actual detailed quest log showing only one quest at a time
    public void SetQuestLogInfo(Quests quest)
    {
        questDisplayNameText.text = quest.info.displayName;

        questStatusText.text = quest.GetFullStatusText();

        // rewards (add this later)
        // experienceRewardsText.text = quest.info.experienceReward + " Gold";
    }
}