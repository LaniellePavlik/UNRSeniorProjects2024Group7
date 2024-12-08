using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCController : MonoBehaviour
{

    public Vector3 spawnPos;
    public int relationshipScore;
    public string scoreSavingFile;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeRelationshipScore(int increase) //use negative value to decrease
    {
        relationshipScore += increase;
    }
}
