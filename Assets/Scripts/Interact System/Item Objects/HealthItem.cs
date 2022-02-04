using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HealthItem : Item
{
    [SerializeField]
    private int healValue = 10;

    public override void OnPickup(GameObject interactable, GameObject interactee)
    {
        base.OnPickup(interactable, interactee);

        interactable.GetComponent<Interactable>().RemoveGameObjectWFeedback();
        interactee.GetComponent<PlayerStats>().Heal(healValue);
    }
}
