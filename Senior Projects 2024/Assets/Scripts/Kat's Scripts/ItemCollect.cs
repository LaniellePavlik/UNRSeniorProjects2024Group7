using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ItemCollect : MonoBehaviour
{
    // Start is called before the first frame update
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
