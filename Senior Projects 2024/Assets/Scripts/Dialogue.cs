using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.Examples;
using LLMUnitySamples;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    public PanelMover textbox;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialogue()
    {
        //confirmed, this method is called
        //Debug.Log("2nd test");

        //FIX ALL, modeled off of MultipleCharacters class
        //public LLMCharacter ghostLLM;
        //public InputField playerText1;
        //public Text AIText1;
        //MultipleCharactersInteraction interaction1;
        //interaction1 = new MultipleCharactersInteraction(playerText1, AIText1, llmCharacter1);
        //interaction1.Start();


        GameEventsManager.instance.miscEvents.PatronTalked();
        textComponent.text = string.Empty;
        index = 0;
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        StartCoroutine(TypeLine());
        // for(int i = 0; i < lines.Length; i++)
        //     NextLine();
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {

        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            textbox.isVisible = false;
            GameEventsManager.instance.playerEvents.EnablePlayerMovement();
        }
    }
    
}