using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Kat
// Manages the player's inventory system
//the inventory manager here is more complex than what you work with in the game because I have not finished implementing 
//everything but i still wanted an inventory system to be added
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    // List to store all items currently in the inventory
    public List<Item> Items = new List<Item>();

    public Transform ItemContent;
    public GameObject InventoryItem;

    // Unity's Awake method, called when the script instance is being loaded
    public void Awake()
    {
        Instance = this;

    }

    // Adds a new item to the inventory list
    public void Add(Item item)
    {
        Items.Add(item);
    }

    //method to list all of the items within the inventory and update the ui accordingly
    public void ListItems()
    { 
        foreach (Transform item in ItemContent)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in Items)
        {
            GameObject obj = Instantiate(InventoryItem, ItemContent);
            var itemName = obj.transform.Find("ItemName").GetComponent<Text>();
            var itemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();

            itemName.text = item.itemName;
            itemIcon.sprite = item.icon;
        }
    }
}
