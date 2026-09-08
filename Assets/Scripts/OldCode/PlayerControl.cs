using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : NetworkBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    public float mouseSensitivity;
    public NetworkVariable<float> movementSpeed = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> jumpPower = new(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Camera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            Debug.Log("IsOwner!");
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerInput.enabled = true;
            playerInput.ActivateInput();
        }
        else
        {
            // Crucial: Disable input for remote clones so you don't control them
            playerInput.enabled = false;
        }
    }

    public void OnMove(InputValue value)
    {
        Debug.Log("Moving");
    }

    public void OnLook(InputValue value)
    {
        Debug.Log("Looking " + value);
    }

    public void OnJump(InputValue value)
    {
        Debug.Log("Jumping");
    }
}
