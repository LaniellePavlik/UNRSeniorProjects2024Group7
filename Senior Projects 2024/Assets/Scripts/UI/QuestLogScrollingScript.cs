using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public QuestLogButton InstantiateQuestLogButton(Quests quest, UnityAction selectAction)
    {
        // create the button
        QuestLogButton questLogButton = Instantiate(
            questLogButtonPrefab,
            contentParent.transform).GetComponent<QuestLogButton>();
        // game object name in the scene
        questLogButton.gameObject.name = quest.info.id + "_button";
        // initialize and set up function for when the button is selected
        RectTransform buttonRectTransform = questLogButton.GetComponent<RectTransform>();
        questLogButton.Initialize(quest.info.displayName, () => {
            selectAction();
            UpdateScrolling(buttonRectTransform);
        });
        // add to map to keep track of the new button
        idToButtonMap[quest.info.id] = questLogButton;
        return questLogButton;
    }

    public void UpdateScrolling(RectTransform buttonRectTransform)
    {
        // calculate the min and max for the selected button
        float buttonYMin = Mathf.Abs(buttonRectTransform.anchoredPosition.y);
        float buttonYMax = buttonYMin + buttonRectTransform.rect.height;

        // calculate the min and max for the content area
        float contentYMin = contentRectTransform.anchoredPosition.y;
        float contentYMax = contentYMin + scrollRectTransform.rect.height;

        // handle scrolling down
        if (buttonYMax > contentYMax)
        {
            contentRectTransform.anchoredPosition = new Vector2(
                contentRectTransform.anchoredPosition.x,
                buttonYMax - scrollRectTransform.rect.height
            );
        }
        // handle scrolling up
        else if (buttonYMin < contentYMin) 
        {
            contentRectTransform.anchoredPosition = new Vector2(
                contentRectTransform.anchoredPosition.x,
                buttonYMin
            );
        }
    }
}