using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private InputSystem_Actions playerInput;

    private void Awake() 
    {
        playerInput = new InputSystem_Actions();

        playerInput.Player.Enable();
    }

    public Vector2 GetMovementValue()
    {
        Vector2 inputVector = playerInput.Player.Move.ReadValue<Vector2>();

        return inputVector;
    }

    public bool IsJumpPressed()
    {
        return playerInput.Player.Jump.WasPressedThisFrame();
    }
}
