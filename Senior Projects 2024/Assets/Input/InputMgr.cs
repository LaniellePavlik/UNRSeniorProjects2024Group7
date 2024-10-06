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
    public void Awake()
    {
        inst = this;
        input = new GameControls();
    }

    private void OnEnable()
    {
        move = input.Movement.Move;
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        player.MovePlayer(move.ReadValue<Vector2>());
    }
}
