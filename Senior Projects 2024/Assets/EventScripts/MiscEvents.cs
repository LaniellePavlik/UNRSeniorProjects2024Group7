using System;

public class MiscEvents
{
    public event Action onPatronTalked;
    public void PatronTalked() 
    {
        if (onPatronTalked != null) 
        {
            onPatronTalked();
        }
    }
}