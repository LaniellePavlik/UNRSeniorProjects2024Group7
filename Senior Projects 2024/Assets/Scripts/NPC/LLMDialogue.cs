using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
// using UnityEditor.VersionControl;
using static UnityEngine.InputSystem.InputRemoting;

//Authored By Lanielle, based on SimpleInteraction example code from LLM Plugin

public class LLMInteraction : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public TMPro.TextMeshProUGUI AIText;
    private int count;
    private int rating;

    void Start()
    {
        count = 0;
    }

    public void welcome(EmptyCallback callback = null)
    {
        //the prompt below should be changed for each NPC, replace with variable later
        _ = llmCharacter.Chat("Welcome Ophelia to the cursed library", SetAIText, callback, false);
    }

    public void getResponse(string message, EmptyCallback callback = null)
    {
        AIText.text = "...";
        _ = llmCharacter.Chat(message, SetAIText, callback);
    }

    //void onInputFieldSubmit(string message)
    //{
    //    count++;
    //    if (count <2)
    //    {
    //        playerText.interactable = false;
    //        AIText.text = "...";
    //        _ = llmCharacter.Chat(message, SetAIText, AIReplyComplete);
    //    }
    //    else
    //    {
    //        EndConversation(message);
    //        count = -1;
    //    }
    //}

    public void SetAIText(string text)
    {
        AIText.text = text;
    }

    //added for debugging purposes - LLMCharacter.Chat() requires callback functions
    public void setRatingVar(string text)
    {
        Debug.Log("LLM: "+text);
        int.TryParse(text, out rating);
    }

    public void SetAIGoodbyeText(string text)
    {
        AIText.text = text + " Goodbye!";
    }

    public void EndConversation(string text, EmptyCallback callback = null)
    {
        //_ = llmCharacter.Chat("respond to the following: \""+text+"\" and say goodbye to Ophelia.",
        //    SetAIText, AIReplyComplete);
        _ = llmCharacter.Chat(text, SetAIGoodbyeText, null);//todo: move setAItext, etc. to Dialogue
        _ = llmCharacter.Chat("Rate the pleasantness of this conversation on a scale from 1 to 10. " +
            "Respond with only the number.", setRatingVar, callback, false);
    }

    public int getRating()
    {
        int r = rating;
        rating = 0;
        return r-5;

    }

    //public void AIReplyComplete()
    //{
    //    playerText.interactable = true;
    //    playerText.Select();
    //    if (count != -1)
    //    {
    //        playerText.text = "";
    //    } 
    //    else
    //    {
    //        playerText.text = "The Ghost seems busy. Use 'enter' to exit.";
    //    }
    //}

    //public void CancelRequests()
    //{
    //    llmCharacter.CancelRequests();
    //    AIReplyComplete();
    //}

    //public void ExitGame()
    //{
    //    Debug.Log("Exit button clicked");
    //    Application.Quit();
    //}

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
