using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn


public class BookAppear : MonoBehaviour
{

    public GameObject book;
    public QuestPoint questPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //This just checks if the quest to enter the book is done, so the book stack appears and can be entered
        if(questPoint.currentQuestState == QuestState.FINISHED)
        {
            book.SetActive(true);
        }
    }
}
