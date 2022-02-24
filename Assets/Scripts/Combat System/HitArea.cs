using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitArea : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerController;

    public PlayerStats PlayerStats { set { playerStats = value; } }
    public PlayerController PlayerController { set { playerController = value; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && playerController.IsAttacking())
        {
            other.GetComponent<EnemyAI>().TakeDamage(playerStats.Damage, other.GetComponent<Transform>());
            Debug.Log("ENEMY HEALTH: " + other.GetComponent<EnemyAI>().Health);
        }
    }
}
