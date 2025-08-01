using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTriggers : MonoBehaviour
{
    //attach to any assets that would change the player's state
    //e.g. a trigger that changes the player to gas state when entered
    //make sure the player has a StatesManager component attached
    public PlayerStates _targetState;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var state = other.GetComponent<StatesManager>();
        if (state != null)
        {
            state.ChangeState(_targetState);
            Debug.Log($"Player state changed to: {state.CurrentState}");
        }
    }
}
