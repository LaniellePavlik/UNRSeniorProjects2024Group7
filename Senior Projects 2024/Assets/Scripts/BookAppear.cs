using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if(questPoint.currentQuestState == QuestState.FINISHED)
        {
            book.SetActive(true);
        }
    }
}
