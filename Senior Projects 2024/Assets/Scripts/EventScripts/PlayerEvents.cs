using System;

//Author: Fenn

public class PlayerEvents
{
    //Disables player movement for when I need it to not be active in a menu or during dialogue
    public event Action onDisablePlayerMovement;
    public void DisablePlayerMovement()
    {
        if (onDisablePlayerMovement != null) 
        {
            onDisablePlayerMovement();
        }
    }

    //Enables player movement for when they're freed of a menu or dialogue
    public event Action onEnablePlayerMovement;
    public void EnablePlayerMovement()
    {
        if (onEnablePlayerMovement != null) 
        {
            onEnablePlayerMovement();
        }
    }
}