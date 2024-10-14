using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatronRequests", menuName = "SciptableObjects/PatronRequests", order = 1)]
public class PatronRequests : ScriptableObject
{
    [field: SerializeField] public string id {get; private set;}

    [Header("General")]
    public string displayName;

    [Header("Requirements")]
    public PatronRequests[] questPrerecs;

    [Header("Steps")]
    public GameObject[] questStepPrefabs;

    [Header("Rewards")]
    public int expGain;

    //Quest steps are broken into pieces we need to do consecutively, not things that can all be done at the same time 
    GameObject[] questSteps;

    int reward;
    
    //Make sure the ID is always the name of scriptable object asset
    private void OnValidate()
    {
        #if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
