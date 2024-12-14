using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.Examples;
using LLMUnitySamples;
using UnityEngine.UI;
using Unity.VisualScripting;

//Authored By Fenn(Quest Part) and Lanielle(LLM Interaction)

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI scriptedTextComponent;
    public float textSpeed;
    public PanelMover textbox;
    public PanelMover NPCtextbox;
    public LLMInteraction LLM;
    public InputField playerText;
    public NPCController npc;
    [HideInInspector] public List<string> codedlines;

    private int count;
    private bool willingToTalk; //get this from NPCs
    private bool llmConvo = false;
    private int index;

    void Start()
    {
        count = 0;
        willingToTalk = true;
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
    }

    void Update()
    {
        //This function updates the dialogue output for the LLM - Lanielle
        if (Input.GetKeyDown(KeyCode.Return) && llmConvo == true)
        {
            count++;
            if (count==5)
            {
                textbox.isVisible = false;
                textComponent.text = string.Empty;
                GameEventsManager.instance.playerEvents.EnablePlayerMovement();
                count = 0;
                willingToTalk = true;
            }
        }

        //This function updates the dialogue output for the LLM - Both
        if (Input.GetKeyDown(KeyCode.Return) && llmConvo == false)
        {
            if (scriptedTextComponent.text == codedlines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                scriptedTextComponent.text = codedlines[index];
            }
        }
    }

    //This function specifically starts the dialogue in relation to the LLM - Lanielle
    public void StartDialogue()
    {
        textbox.isVisible = true;
        GameEventsManager.instance.miscEvents.PatronTalked();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        playerText.text = "";
        llmConvo = true;
        LLM.welcome(AIReplyComplete);
    }

    //This function specifically starts the dialogue in relation to the start of quests -Fenn
    public void StartQuestDialogue(List<string> quest)
    {
        NPCtextbox.isVisible = true;
        GameEventsManager.instance.miscEvents.PatronTalked();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        scriptedTextComponent.text = "";
        codedlines.Clear();
        codedlines = quest;
        index = 0;
        llmConvo = false;
        StartCoroutine(TypeLine());
    }

    //This function specifically starts the dialogue in relation to the end of quests -Fenn
    public void EndQuestDialogue(List<string> quest)
    {
         NPCtextbox.isVisible = true;
        GameEventsManager.instance.miscEvents.PatronTalked();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        scriptedTextComponent.text = "";
        codedlines.Clear();
        codedlines = quest;
        index = 0;
        llmConvo = false;
        StartCoroutine(TypeLine());
    }

    //Continue the conversation for three player inputs then forefully initiate a goodbye.
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

    //This function will end the AI request and change the player's relationship score - Lanielle
    public void AIReplyComplete()
    {
        if (willingToTalk)
        {
            playerText.text = "";
            playerText.interactable = true;
            playerText.Select();
        }
        else
        {
            Debug.Log("!!!");
            npc.changeRelationshipScore(LLM.getRating());
        }
    }

    //This function iterates through the characters in the dialogue string of each line. - Fenn
    IEnumerator TypeLine()
    {
        foreach (char c in codedlines[index].ToCharArray())
        {
            scriptedTextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    //This function goes to next line in the list of strings to continue dialogue - Fenn
    void NextLine()
    {
        if (index < codedlines.Count - 1)
        {
            index++;
            scriptedTextComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            NPCtextbox.isVisible = false;
            GameEventsManager.instance.playerEvents.EnablePlayerMovement();
        }
    }
}