using LLMUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyScript
{
    public LLMCharacter llmCharacter;

    void HandleReply(string reply)
    {
        // do something with the reply from the model
        Debug.Log(reply);
    }
    void ReplyCompleted()
    {
        // do something when the reply from the model is complete
        Debug.Log("The AI replied");
    }

    void Game()
    {
        // your game function
        string message = "Hello bot!";
        _ = llmCharacter.Chat(message, HandleReply);
    }







}
