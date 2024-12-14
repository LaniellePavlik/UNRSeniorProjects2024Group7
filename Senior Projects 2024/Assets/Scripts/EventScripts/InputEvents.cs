using UnityEngine;
using System;

//Author: Fenn

public class InputEvents
{
    //Action that will register the player has pressed E
    public event Action onSubmitPressed;
    public void SubmitPressed()
    {
        if (onSubmitPressed != null) 
        {
            onSubmitPressed();
        }
    }

    //Action that will register the player has pressed E specifically to get into the book
    public event Action onBookSubmitPressed;
    public void SubmitBookPressed()
    {
        if (onBookSubmitPressed != null)
        {
            onBookSubmitPressed();
        }
    }

    //Action that will register the player has pressed Q to specifically bring up the full quest log
    public event Action onQuestLogTogglePressed;
    public void QuestLogTogglePressed()
    {
        if (onQuestLogTogglePressed != null) 
        {
            onQuestLogTogglePressed();
        }
    }
}
