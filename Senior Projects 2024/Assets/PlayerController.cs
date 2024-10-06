using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MovePlayer(Vector2 input)
    {
        Vector3 moveVector = new Vector3(input.x * Time.deltaTime, 0, input.y * Time.deltaTime) * moveSpeed;

        transform.position += moveVector;
    }
}
