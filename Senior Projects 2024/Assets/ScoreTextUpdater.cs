using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTextUpdater : MonoBehaviour
{
    // Start is called before the first frame update
    public NPCController controller;
    public TextMeshProUGUI text;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Relationship: " + controller.relationshipScore;
    }
}
