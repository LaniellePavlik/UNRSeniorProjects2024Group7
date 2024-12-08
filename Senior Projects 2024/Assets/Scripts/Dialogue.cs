using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.Examples;
using LLMUnitySamples;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float textSpeed;
    public PanelMover textbox;
    public LLMInteraction LLM;
    public InputField playerText;

    private int count;
    private bool willingToTalk; //get this from NPCs

    void Start()
    {
        count = 0;
        willingToTalk = true;
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            count++;
            if (count==5)
            {
                Debug.Log("!!!");
                textbox.isVisible = false;
                textComponent.text = string.Empty;
                GameEventsManager.instance.playerEvents.EnablePlayerMovement();
                count = 0;
                willingToTalk = true;
            }

        }
    }

    public void StartDialogue()
    {
        GameEventsManager.instance.miscEvents.PatronTalked();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        playerText.text = "";
        LLM.welcome(AIReplyComplete);
    }

    public void onInputFieldSubmit(string message)
    {
        playerText.interactable = false;
        if (count > 2)
        {   
            playerText.text = "The Ghost seems busy. Use 'enter' to exit.";
            willingToTalk = false;
            LLM.EndConversation(message, AIReplyComplete);
        }
        else
        {
            LLM.getResponse(message, AIReplyComplete);
        }
    }

    public void AIReplyComplete()
    {
        if (willingToTalk)
        {
            playerText.text = "";
            playerText.interactable = true;
            playerText.Select();
        }
    }
}