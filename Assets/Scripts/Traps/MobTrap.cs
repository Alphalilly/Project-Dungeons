using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobTrap : MonoBehaviour
{
    public StructureBehavior structure;
    public GameObject playerObj;
    public PlayerController player;
    public List<GameObject> enemys = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (structure == null)
        {
            structure = GetComponent<StructureBehavior>();
        }

        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
            player = playerObj.GetComponent<PlayerController>();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject enemy in enemys)
            {
                if (enemy.activeSelf == false)
                {
                    enemys.Remove(enemy);                    
                }           
            }  
            
            if (enemys.Count != 0)
            {
                structure.trapPlayer = true;
            }

            else if (enemys.Count == 0)
            {
                structure.trapPlayer = false;
            }
        }
    }
}
