using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

    public void ShowUI()
    {
        UIpanel.isVisible = true;
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        if (firstSelectedButton != null)
        {
            firstSelectedButton.Select();
        }
    }

    public void HideUI()
    {
        UIpanel.isVisible = false;
        GameEventsManager.instance.playerEvents.EnablePlayerMovement();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void QuestStateChange(Quests quest)
    {
        // add the button to the scrolling list if not already added
        QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () => {
            SetQuestLogInfo(quest);
        });

        if (firstSelectedButton == null)
        {
            firstSelectedButton = questLogButton.button;
        }

        // set the button color based on quest state
        questLogButton.SetState(quest.state);
    }

    public void SetQuestLogInfo(Quests quest)
    {
        // quest name
        questDisplayNameText.text = quest.info.displayName;

        // status
        questStatusText.text = quest.GetFullStatusText();

        // rewards (add this later)
        // experienceRewardsText.text = quest.info.experienceReward + " Gold";
    }
}