using UnityEngine;

//Author: Kat
//using the scriptable object system
//script defines the properties of an inventory item
//like the name identification number boost value
[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
public class Item : ScriptableObject
{
    public int id;
    public string itemName;
    public int value;
    public Sprite icon;
    public string itemDescription;
}