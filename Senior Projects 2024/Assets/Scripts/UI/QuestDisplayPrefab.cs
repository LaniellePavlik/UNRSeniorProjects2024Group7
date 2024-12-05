using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestDisplayPrefab : MonoBehaviour
{

    public GameObject display { get; private set; }

    public TextMeshProUGUI QuestTitle;
    public TextMeshProUGUI QuestDescription;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void UpdateDisplay(string displayName, string questText) 
    {
        QuestTitle.text = displayName;
        QuestDescription.text = questText;
    }

}
