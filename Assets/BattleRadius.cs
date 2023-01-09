using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRadius : MonoBehaviour
{
    public bool defeated = false;
    public bool isBoss = false;
    GameObject playerObj;
    PlayerController player;

    private void Start()
    {

    }

    private void Update()
    {
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
            player = playerObj.GetComponent<PlayerController>();
        }    
    }

    private void OnTriggerStay(Collider other)
    {
        if (isBoss)
        {
            if (other.gameObject.CompareTag("Player") && defeated == false)
            {
                player.activeBossCamera = true;
            }

            else if (other.gameObject.CompareTag("Player") && defeated == true)
            {
                player.activeBossCamera = false;
            }
        }

        else
        {
            if (other.gameObject.CompareTag("Player") && defeated == false)
            {
                player.activeBattleCamera = true;
            }

            else if (other.gameObject.CompareTag("Player") && defeated == true)
            {
                player.activeBattleCamera = false;
            }
        }    
    }

    private void OnTriggerExit(Collider other)
    {
        if (isBoss)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                player.activeBossCamera = false;
            }
        }

        else
        {
            if (other.gameObject.CompareTag("Player"))
            {
                player.activeBattleCamera = false;
            }
        }        
    }
}
