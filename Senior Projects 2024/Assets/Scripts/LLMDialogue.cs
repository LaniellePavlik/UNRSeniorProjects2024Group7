using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using UnityEditor.VersionControl;


public class SimpleInteraction : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public TMPro.TextMeshProUGUI AIText;
    private int count;

    void Start()
    {
        count = 0;
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
        //the prompt below should be changed for each NPC, replace with variable later
        _ = llmCharacter.Chat("Welcome Ophelia to the cursed library", SetAIText, AIReplyComplete, false);
    }

    void onInputFieldSubmit(string message)
    {
        count++;
        Debug.Log(count);
        if (count <2)
        {
            playerText.interactable = false;
            AIText.text = "...";
            _ = llmCharacter.Chat(message, SetAIText, AIReplyComplete);
        }
        else
        {
            EndConversation(message);
            count = -1;
        }
    }

    public void SetAIText(string text)
    {
        AIText.text = text;
        //Debug.Log(text);
    }

    //added for debugging purposes - LLMCharacter.Chat() requires callback functions
    public void PrintAIText(string text)
    {
        Debug.Log(text);
    }

    public void SetAIGoodbyeText(string text)
    {
        AIText.text = text + " Goodbye!";
    }

    public void EndConversation(string text)
    {
        //_ = llmCharacter.Chat("respond to the following: \""+text+"\" and say goodbye to Ophelia.",
        //    SetAIText, AIReplyComplete);
        _ = llmCharacter.Chat(text+"#", SetAIGoodbyeText, AIReplyComplete);
        _ = llmCharacter.Chat("Rate the pleasantness of this conversation on a scale from 1 to 10. " +
            "Respond with only the number.", PrintAIText, null, false);
    }

    public void AIReplyComplete()
    {
        playerText.interactable = true;
        playerText.Select();
        if (count != -1)
        {
            playerText.text = "";
        } 
        else
        {
            playerText.text = "The Ghost seems busy. Use 'enter' to exit.";
        }
    }

    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
        AIReplyComplete();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    bool onValidateWarning = true;
    void OnValidate()
    {
        if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
        {
            Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            onValidateWarning = false;
        }
    }
}
