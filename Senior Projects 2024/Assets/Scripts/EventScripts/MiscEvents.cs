using System;

//Author: Fenn

public class MiscEvents
{
    //This action aids the dialogue and talk to patron quests, I didn't really know where to put it lol
    public event Action onPatronTalked;
    public void PatronTalked() 
    {
        if (onPatronTalked != null) 
        {
            onPatronTalked();
        }
    }
}