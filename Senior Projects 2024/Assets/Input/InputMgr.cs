using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public void Awake()
    {
        inst = this;
        input = new GameControls();
    }

    private void OnEnable()
    {
        move = input.Movement.Move;
        move.Enable();

        cursorPos = input.Attack.CursorPos;
        cursorPos.Enable();

        dash = input.Movement.Dash;
        dash.Enable();
        dash.performed += StartDash;

        regularAttack = input.Attack.RegularAttack;
        regularAttack.performed += RegularAttack;
        regularAttack.Enable();

        interact = input.Interaction.Interact;
        interact.performed += Interact;
        interact.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        cursorPos.Disable();
        dash.Disable();
        regularAttack.Disable();
        interact.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
}
