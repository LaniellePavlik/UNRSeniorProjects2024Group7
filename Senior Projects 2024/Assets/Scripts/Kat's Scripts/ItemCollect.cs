using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

//Author: Kat
//it just adds a new item into the inventory
//the tutorial i used was for collecting prefabs and adding them to the inventory
//but I am trying to figure how to implement where you get the item after accomplishing a goal
//maybe have it connected to fenn's quest system
public class ItemCollect : MonoBehaviour
{
    public Item Item;

    void Collect()
    {
        InventoryManager.Instance.Add(Item);
    }

    private void CompleteBook()
    {
        Collect();
    }
}
