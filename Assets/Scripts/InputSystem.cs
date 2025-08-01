using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public static InputSystem current;
    public InputSystem_Actions actions;
    private void Awake()
    {
        current = this;
        actions = new InputSystem_Actions();
        actions.Enable();
        actions.Player.Enable();
    }
}
