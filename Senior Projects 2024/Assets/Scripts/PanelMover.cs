using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Fenn

public class PanelMover : MonoBehaviour
{

    public Vector2 holdingPosition;
    public Vector2 visiblePosition = Vector2.zero;
    public RectTransform panel;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        // holdingPosition = panel.anchoredPosition;
    }

    //If the bool is set to visible it will move the panel onto the canvas, and vice versa if false
    public bool _isVisible = false;
    public bool isVisible
    {
        get { return _isVisible; }
        set
        {
            _isVisible = value;
            if(_isVisible) {
                panel.anchoredPosition = visiblePosition;
                if(isTimed)
                    StartCoroutine(HideAfterTime(showDuration));
            } else {
                panel.anchoredPosition = holdingPosition;
            }
        }
    }

    public bool isTimed = false; //Option for panel to be timed in its length.
    public float showDuration = 5;

    IEnumerator HideAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        isVisible = false;
    }

    //Context menu's to move panels around in the editor so I don't have to drag them all the time

    [ContextMenu("TestPanel")]
    public void TestPanel()
    {
        isVisible = true;
    }

    [ContextMenu("DisablePanel")]
    public void DisablePanel()
    {
        isVisible = false;
    }

    [ContextMenu("SetPos")]
    public void SetPos()
    {
        SetVisiblePosition(visiblePosition);
    }

    // Sets the visible position
    public void SetVisiblePosition(Vector2 position)
    {
        visiblePosition = position;
        panel.anchoredPosition = visiblePosition;
    }

    // Pauses the time for when you're in a menu
    public void pauseTime()
    {
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        Time.timeScale = 0;
    }

    // Starts the time for when you're out of a menu
    public void startTime()
    {
        GameEventsManager.instance.playerEvents.EnablePlayerMovement();
        Time.timeScale = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}