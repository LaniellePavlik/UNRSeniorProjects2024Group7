using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Fenn
//With Reference to: https://www.youtube.com/watch?v=UyTJLDGcT64 
//This file holds all the data for a scriptable object quest which I have decided is named PatronRequests

[CreateAssetMenu(fileName = "PatronRequests", menuName = "SciptableObjects/PatronRequests", order = 1)]
public class PatronRequests : ScriptableObject
{
    //Declare all the vairiables we need
    [field: SerializeField] public string id {get; private set;}

    [Header("General")]
    public string displayName;

    [Header("Requirements")]
    public PatronRequests[] questPrerecs;

    [Header("Steps")]
    public GameObject[] questStepPrefabs;

    [Header("Rewards")]
    public int goldReward;
    
    GameObject[] questSteps;

    int reward;
    
    //Verify the Id is the same
    private void OnValidate()
    {
        #if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
