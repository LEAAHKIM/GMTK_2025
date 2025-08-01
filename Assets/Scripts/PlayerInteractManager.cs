using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteractManager : MonoBehaviour
{
    public static PlayerInteractManager current;
    private int lastHoverInteractableIndex=-1;
    private bool interactKeyPressed;
    private float _lastInteractKeyPressedTime = -1;
    private bool interactKeyBufferingActive { get { return Time.time - _lastInteractKeyPressedTime <= 0.2f; } }

    private void Awake()
    {
        current = this;
    }
    private List<Interactable> interactables = new List<Interactable>();
    private void Start()
    {
        InputSystem.current.actions.Player.InteractKeyPressed.performed += ctx => { interactKeyPressed = true; };
    }
    public void AddInteractable(Interactable a)
    {
        interactables.Add(a);
    }
    public void RemoveInteractable(int uid)
    {
        if (lastHoverInteractableIndex > -1) { interactables[lastHoverInteractableIndex].onStopHover?.Invoke(); }
        // this will be short anyways, O(n) is fine
        for (int i = interactables.Count - 1; i >= 0; i--)
        {
            if (interactables[i].uid == uid) { interactables.RemoveAt(i); }
        }

        lastHoverInteractableIndex = -1;
    }
    private void FixedUpdate()
    {
        RunInteractables();
        void RunInteractables()
        {
            if (interactKeyPressed) { _lastInteractKeyPressedTime = Time.time; }
            float shortestDistance = Mathf.Infinity;
            int shortestDistInteractableIndex = -1;
            for (int i = 0; i < interactables.Count; i++)
            {
                float dist = Vector2.Distance(transform.position, interactables[i].transform.position);
                if (dist < 6 && dist < shortestDistance)
                {
                    shortestDistance = dist;
                    shortestDistInteractableIndex = i;
                }
            }
            if (lastHoverInteractableIndex != shortestDistInteractableIndex)
            {
                if (lastHoverInteractableIndex > -1) { interactables[lastHoverInteractableIndex].onStopHover?.Invoke(); }
                if (shortestDistInteractableIndex > -1) { interactables[shortestDistInteractableIndex].onHover?.Invoke(); }
            }
            if (shortestDistInteractableIndex > -1 && interactKeyBufferingActive)
            {
                _lastInteractKeyPressedTime = -1;
                interactables[shortestDistInteractableIndex].onInteract?.Invoke();
            }
            lastHoverInteractableIndex = shortestDistInteractableIndex;
            interactKeyPressed = false;
        }
    }

}
