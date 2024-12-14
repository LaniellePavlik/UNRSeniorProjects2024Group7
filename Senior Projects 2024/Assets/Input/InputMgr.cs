//Script: InputMgr.cs
//Contributors: Liam Francisco and Fenn Edmonds
//Summary: Handles all inputs from player
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputMgr : MonoBehaviour
{
    public static InputMgr inst;

    public PlayerController player;
    private GameControls input;
    private InputAction move;
    private InputAction dash;
    private InputAction cursorPos;
    private InputAction regularAttack;
    private InputAction interact;
    private InputAction questLog;
    private Scene currentScene;
    private bool canDisable = false;

    //sets up an instance of the InputMgr and creates a new reference to the inputs
    public void Awake()
    {
        inst = this;
        input = new GameControls();
    }

    void Start()
    {
        
    }

    // turns the inputs on when the game loads the scene.
    private void OnEnable()
    {
        move = input.Movement.Move;
        move.Enable();

        cursorPos = input.Attack.CursorPos;
        cursorPos.Enable();

        dash = input.Movement.Dash;
        dash.Enable();
        dash.performed += StartDash;

        currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        if(sceneName != "Fenn")
        {
            canDisable = true;
            regularAttack = input.Attack.RegularAttack;
            regularAttack.performed += RegularAttack;
            regularAttack.Enable();
        }

        interact = input.Interaction.Interact;
        interact.performed += Interact;
        interact.performed += SubmitPressed;
        interact.Enable();

        questLog = input.Interaction.QuestLogToggle;
        questLog.performed += QuestLogTogglePressed;
        questLog.Enable();

        GameEventsManager.instance.playerEvents.onDisablePlayerMovement += DisablePlayerMovement;
        GameEventsManager.instance.playerEvents.onEnablePlayerMovement += EnablePlayerMovement;
    }

    //turns the inputs off when the game loads the scene. Unity’s input system recommends this to avoid errors when loading new scenes
    private void OnDisable()
    {
        move.Disable();
        cursorPos.Disable();
        dash.Disable();
        if(canDisable == true)
            regularAttack.Disable();
        interact.Disable();
        
        GameEventsManager.instance.playerEvents.onDisablePlayerMovement -= DisablePlayerMovement;
        GameEventsManager.instance.playerEvents.onEnablePlayerMovement -= EnablePlayerMovement;
    }

    // handles reading values that constantly need to be read, such as mouse position on the screen
    void Update()
    {
        player.MovePlayer(move.ReadValue<Vector2>());
        player.ChangeDirection(cursorPos.ReadValue<Vector2>());
    }

    private void StartDash(InputAction.CallbackContext context)
    {
        player.StartDash(cursorPos.ReadValue<Vector2>(), move.ReadValue<Vector2>());
    }

    private void RegularAttack(InputAction.CallbackContext context)
    {
        player.StartAttack();
    }

    private void Interact(InputAction.CallbackContext context)
    {
        player.Interact();
    }

    public void SubmitPressed(InputAction.CallbackContext context)
    {
        GameEventsManager.instance.inputEvents.SubmitPressed();
    }

    private void DisablePlayerMovement() 
    {
        move.Disable();
        cursorPos.Disable();
        dash.Disable();
        interact.Disable();
    }

    private void EnablePlayerMovement() 
    {
        move.Enable();
        cursorPos.Enable();
        dash.Enable();
        interact.Enable();
    }

    public void QuestLogTogglePressed(InputAction.CallbackContext context)
    {
        GameEventsManager.instance.inputEvents.QuestLogTogglePressed();
    }
}
