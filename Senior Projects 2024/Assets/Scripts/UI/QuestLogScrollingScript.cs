using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Author:Fenn
//Reference: https://www.youtube.com/watch?v=ZYVED_aLHj0&t=503s with modifications
//This handles the scrolling quest log
public class QuestLogScrollingList : MonoBehaviour
{
    [Header("Components")]
    public GameObject contentParent;

    [Header("Rect Transforms")]
    public RectTransform scrollRectTransform;
    public RectTransform contentRectTransform;

    [Header("Quest Log Button")]
    public GameObject questLogButtonPrefab;

    private Dictionary<string, QuestLogButton> idToButtonMap = new Dictionary<string, QuestLogButton>();

    //Create the selectable questbuttons
    public QuestLogButton CreateButtonIfNotExists(Quests quest, UnityAction selectAction) 
    {
        QuestLogButton questLogButton = null;
        // only create the button if id hasn't been seen before
        if (!idToButtonMap.ContainsKey(quest.info.id))
        {
            questLogButton = InstantiateQuestLogButton(quest, selectAction);
        }
        else 
        {
            questLogButton = idToButtonMap[quest.info.id];
        }
        return questLogButton;
    }

    //Actually instantiate the buttons
    public QuestLogButton InstantiateQuestLogButton(Quests quest, UnityAction selectAction)
    {
        QuestLogButton questLogButton = Instantiate(
            questLogButtonPrefab,
            contentParent.transform).GetComponent<QuestLogButton>();
        questLogButton.gameObject.name = quest.info.id + "_button";
        //init and set up button
        RectTransform buttonRectTransform = questLogButton.GetComponent<RectTransform>();
        questLogButton.Initialize(quest.info.displayName, () => {
            selectAction();
            UpdateScrolling(buttonRectTransform);
        });
        // add to map to keep track of the new button
        idToButtonMap[quest.info.id] = questLogButton;
        return questLogButton;
    }

    //If there is more quests than space in the scroll, update the quest scrolling port!
    public void UpdateScrolling(RectTransform buttonRectTransform)
    {
        //Calc the min and max for the selected button
        float buttonYMin = Mathf.Abs(buttonRectTransform.anchoredPosition.y);
        float buttonYMax = buttonYMin + buttonRectTransform.rect.height;

        //CAlcthe min and max for the content area
        float contentYMin = contentRectTransform.anchoredPosition.y;
        float contentYMax = contentYMin + scrollRectTransform.rect.height;

        //Scrolling down
        if (buttonYMax > contentYMax)
        {
            contentRectTransform.anchoredPosition = new Vector2(
                contentRectTransform.anchoredPosition.x,
                buttonYMax - scrollRectTransform.rect.height
            );
        }
        //Scrolling up
        else if (buttonYMin < contentYMin) 
        {
            contentRectTransform.anchoredPosition = new Vector2(
                contentRectTransform.anchoredPosition.x,
                buttonYMin
            );
        }
    }
}