using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.Examples;
using LLMUnitySamples;
using UnityEngine.UI;
using Unity.VisualScripting;

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

    public void StartDialogue()
    {
        textbox.isVisible = true;
        GameEventsManager.instance.miscEvents.PatronTalked();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        playerText.text = "";
        llmConvo = true;
        LLM.welcome(AIReplyComplete);
    }

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
        else
        {
            Debug.Log("!!!");
            npc.changeRelationshipScore(LLM.getRating());
        }
    }

    IEnumerator TypeLine()
    {
        foreach (char c in codedlines[index].ToCharArray())
        {
            scriptedTextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

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