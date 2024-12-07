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
    private int count;

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            count++;
            //if (textComponent.text == lines[index])
            //{
            //    NextLine();
            //}
            //else
            //{
            //    StopAllCoroutines();
            //    textComponent.text = lines[index];
            //}

            if (count > 2)
            {
                textbox.isVisible = false;
                GameEventsManager.instance.playerEvents.EnablePlayerMovement();
                count = 0;
            }
        }
    }

    public void StartDialogue()
    {
        //to do: figure out what following line does
        GameEventsManager.instance.miscEvents.PatronTalked();
        //textComponent.text = string.Empty;
        index = 0;
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        //StartCoroutine(TypeLine());
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