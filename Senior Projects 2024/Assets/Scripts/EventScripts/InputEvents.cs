using UnityEngine;
using System;

public class InputEvents
{
    public event Action onSubmitPressed;
    public void SubmitPressed()
    {
        if (onSubmitPressed != null) 
        {
            onSubmitPressed();
        }
    }

    public event Action onBookSubmitPressed;
    public void SubmitBookPressed()
    {
        if (onBookSubmitPressed != null)
        {
            onBookSubmitPressed();
        }
    }

    public event Action onQuestLogTogglePressed;
    public void QuestLogTogglePressed()
    {
        if (onQuestLogTogglePressed != null) 
        {
            onQuestLogTogglePressed();
        }
    }
}
