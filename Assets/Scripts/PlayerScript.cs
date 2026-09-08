using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField] Rigidbody rb;

    [Header("Player Preferences")]
    [Range(0f, 1f)]
    [SerializeField] float mouseSensitivity;

    [Header("Client Specific Objects")]
    [SerializeField] GameObject playerCam;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Animator animator;

    [Header("Player Stats")]
    [SerializeField] private float playerSpeed;
    public NetworkVariable<int> playerHealth = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] Gun equippedGun;

    private UnityEvent onDeath = new UnityEvent();
    private Coroutine firingLoopCoroutine = null;

    private float upDownRotation = 45f;
    private bool isMoving = false;

    private Vector2 flatMoveDirection = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;

    
    [SerializeField] private LayerMask playerBodyLayer;
    [SerializeField] private LayerMask shootThroughLayers;

/*
    private void Awake()
    {
        onDeath = new UnityEvent();
    }*/

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        onDeath.AddListener(OnPlayerDeath);
        shootThroughLayers += playerBodyLayer; // We will also shoot through the player

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCam.SetActive(true);
            playerInput.enabled = true;
        }

    }

    public void OnFire(InputValue value)
    {

        
        if (!IsOwner)
            return;

            animator.SetFloat("FireSpeed", equippedGun.GetFireRateValue()/5);

        if (!equippedGun.GetIsHoldToShoot() && value.isPressed)
        {
            FireRpc();
            return;
        }

        if (value.isPressed)
        {
            if (firingLoopCoroutine == null)
            {
                firingLoopCoroutine = StartCoroutine(FireHeldLoop());
            }
        } else
        {
            if (firingLoopCoroutine != null)
            {
                StopCoroutine(firingLoopCoroutine);
                firingLoopCoroutine = null;
            }
        }

    }

    private IEnumerator FireHeldLoop()
    {
        while (true)
        {
            FireRpc();
            yield return new WaitForSeconds(1 / equippedGun.GetFireRateValue());
        }
    }

    [Rpc(SendTo.Everyone)]
    void FireRpc()
    {
        
        animator.SetTrigger("Fire");
        Debug.Log("Fire! from " + OwnerClientId);

        Vector3 shootingOrigin = playerCam.transform.position;
        Vector3 direction = playerCam.transform.forward;
        float remainingWallPenetration = equippedGun.GetMaxPenetrationDepth();

        TryPenetrateWall(shootingOrigin, direction, remainingWallPenetration, 100);


    }

    private void TryPenetrateWall(Vector3 origin, Vector3 direction, float remainingPenetration, float remainingDistance)
    {
        if (remainingDistance <= 0) {
            Debug.Log("This bullet has fallen off at distance");
            return;
        }

        RaycastHit hit;
        // Logic for detecting thickness of wall
        if(Physics.Raycast(origin, direction, out hit, remainingDistance, shootThroughLayers)) {
            Debug.Log("Hit at " + hit.point);
            for (float i = 0; i < remainingPenetration; i += 0.2f)
            {
                RaycastHit hitExit;
                if(Physics.Raycast(hit.point + direction * i, -direction, out hitExit, remainingPenetration, shootThroughLayers))
                {
                    remainingPenetration -= Vector3.Distance(hit.point, hitExit.point);
                    remainingDistance -= Vector3.Distance(origin, hitExit.point);
                    Debug.Log("Wall penetrated");
                    TryPenetrateWall(hitExit.point, direction, remainingPenetration, remainingDistance);
                    return;
                }
                else
                {
                    continue;
                }

            }

            Debug.Log("Wall too thick");
        }
    }


    public void OnLook(InputValue value)
    {
        if (!IsOwner)
            return;

        Vector2 lookDelta = value.Get<Vector2>();

        transform.Rotate(lookDelta.x * Vector3.up * mouseSensitivity);

        upDownRotation -= lookDelta.y * mouseSensitivity;
        upDownRotation = Mathf.Clamp(upDownRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(upDownRotation, 0f, 0f);
        SetMoveDirection();
    }

    // For Getting Player Input for movement
    public void OnMove(InputValue value)
    {
        if (IsOwner)
        {
            flatMoveDirection = value.Get<Vector2>();

            animator.SetFloat("Vertical", flatMoveDirection.y);
            animator.SetFloat("Horizontal", flatMoveDirection.x);

            if (flatMoveDirection == Vector2.zero)
            {
                isMoving = false;
                return;
            }
            isMoving = true;
            SetMoveDirection();
        }
    }

    // For setting player movement relative to direction faced
    private void SetMoveDirection()
    {
        moveDirection = (flatMoveDirection.x * transform.right + flatMoveDirection.y * transform.forward) * playerSpeed;
    }

    // For moving the player in FixedUpdate
    private void MovePlayer()
    {
        if (IsOwner)
        {
            if (isMoving)
            {
                moveDirection.y += rb.linearVelocity.y;
                rb.linearVelocity = moveDirection;
            }
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("You're dead");
    }

    [Rpc(SendTo.Server)]
    public void ModifyHealthRPC(int amount)
    {
        playerHealth.Value += amount;
        if (playerHealth.Value <= 0)
        {
            onDeath.Invoke();
        }
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }
}
