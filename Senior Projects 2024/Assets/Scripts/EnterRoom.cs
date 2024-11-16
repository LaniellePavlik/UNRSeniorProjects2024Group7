using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterRoom : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //ok so like... either i will need to make seperate files for each seperate music instance or i can figure out how to
        ////if else between library, books, and bosses.
        ////thats to say this does not work lol
        //if (other.CompareTag("Player"))
        //{
        //    AudioMgr.Instance.PlayMusic("Library");

        //}

    }
}
