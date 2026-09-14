using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.UI.Image;

public class PlayerScript : NetworkBehaviour
{

    const int MAX_SHOOT_RANGE = 100;

    [SerializeField] GameObject playerCamera;
    [SerializeField] Rigidbody rb;

    [Header("Player Preferences")]
    [Range(0f, 1f)]
    [SerializeField] float mouseSensitivity;

    [Header("Client Specific Objects")]
    [SerializeField] private GameObject playerCam;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private NetworkTransform playerNetTransform;
    [SerializeField] private SkinnedMeshRenderer playerSkinMeshRenderer;
    [SerializeField] private AudioSource gunAudioSource;
    private PlayerUI playerUI;

    [Header("Player Stats")]
    [SerializeField] private float playerSpeed;
    public NetworkVariable<int> playerHealth = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] Gun equippedGun;

    private UnityEvent onDeath = new UnityEvent();
    private Coroutine firingLoopCoroutine = null;

    private float upDownRotation = 45f;
    private bool isMoving = false;

    private Vector2 flatMoveDirection = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;

    Dictionary<ulong, HashSet<Collider>> playerColliders = new Dictionary<ulong, HashSet<Collider>>();

    [SerializeField] private LayerMask playerBodyLayer;
    [SerializeField] private LayerMask shootThroughLayers;

    [SerializeField] Material[] possiblePlayerSkins;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        shootThroughLayers += playerBodyLayer; // We will also shoot through the player


        playerSkinMeshRenderer.material = possiblePlayerSkins[OwnerClientId];

        Collider[] spawnedPlayerColliders = GetComponentsInChildren<Collider>(includeInactive: true);
        playerColliders[OwnerClientId] = new HashSet<Collider>(spawnedPlayerColliders);

        if (IsOwner)
        {
            playerName.OnValueChanged += OnNameChanged;
            playerName.Value = MenuData.playerName;


            playerHealth.OnValueChanged += OnHealthChanged;
            onDeath.AddListener(OnPlayerDeath);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCam.SetActive(true);
            playerInput.enabled = true;


            playerUI = Instantiate(playerUIPrefab).GetComponent<PlayerUI>();
            playerUI.UpdateHealthPoints(playerHealth.Value);
            playerUI.playerScript = this;

            TeleportRandomSpawn();

        }

    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            playerColliders.Remove(OwnerClientId);
        }
    }


    public void OnFire(InputValue value)
    {
        animator.SetFloat("FireSpeed", equippedGun.GetFireRateValue()/5);

        if (!equippedGun.GetIsHoldToShoot() && value.isPressed)
        {
            Fire();
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
            Fire();
            yield return new WaitForSeconds(1 / equippedGun.GetFireRateValue());
        }
    }

    void Fire()
    {
        Debug.DrawLine(playerCam.transform.position, playerCam.transform.forward * MAX_SHOOT_RANGE);
        networkAnimator.SetTrigger("Fire");

        Vector3 shootingOrigin = playerCam.transform.position;
        Vector3 direction = playerCam.transform.forward;
        float maxPenetration = equippedGun.GetMaxPenetrationDepth();
        ulong shooterId = OwnerClientId;

        AudioManager.Instance.PlaySoundFromSource(AudioManager.Sounds.shootCrunch, gunAudioSource);

        Debug.Log(OwnerClientId + " shot");

        //TryPenetrateWallRPC(gameObject.transform, equippedGun, shootingOrigin, direction, remainingWallPenetration, 100);
        TryServerShootRPC(shootingOrigin, direction, shooterId, equippedGun.GetDamageValue(), maxPenetration, MAX_SHOOT_RANGE);

    }



    [Rpc(SendTo.Server)]
    private void TryServerShootRPC(Vector3 origin, Vector3 direction, ulong shooterId, int gunDamage, float totalPenetration, float maxDistance)
    {
        bool isWallbang = false;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, shootThroughLayers, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits) {
            Debug.Log(hit.collider.name);

            if (IsShooterCollider(shooterId, hit.collider)) continue;
            

            if ((playerBodyLayer.value & (1 << hit.collider.transform.gameObject.layer)) != 0)
            {
                if (isWallbang) // Only hit players through walls
                {
                    PlayerScript hitPlayerScript = hit.transform.root.GetComponent<PlayerScript>();
                    hitPlayerScript.playerHealth.Value -= gunDamage;
                }
            } else
            {
                isWallbang = true;
            }

                Ray exitCheckRay = new Ray(hit.point + direction * totalPenetration, -direction);
            if (hit.collider.Raycast(exitCheckRay, out RaycastHit exitHit, totalPenetration))
            {
                totalPenetration -= Vector3.Distance(hit.point, exitHit.point);
                if (totalPenetration <= 0)
                {
                    Debug.Log("Wall too thick");
                    return;
                }
            } else
            {
                Debug.Log("Wall too thick");
                return;
            }

        }
    }

    public bool IsPlayerInCrosshair()
    {
        bool isWallbang = false;
        Vector3 shootingOrigin = playerCam.transform.position;
        Vector3 direction = playerCam.transform.forward;
        float maxPenetration = equippedGun.GetMaxPenetrationDepth();
        ulong shooterId = OwnerClientId;

        RaycastHit[] hits = Physics.RaycastAll(shootingOrigin, direction, MAX_SHOOT_RANGE, shootThroughLayers, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (IsShooterCollider(shooterId, hit.collider)) continue;


            if ((playerBodyLayer.value & (1 << hit.collider.transform.gameObject.layer)) != 0)
            {
                if (isWallbang) return true;
            }
            else isWallbang = true;

            Ray exitCheckRay = new Ray(hit.point + direction * maxPenetration, -direction);
            if (hit.collider.Raycast(exitCheckRay, out RaycastHit exitHit, maxPenetration))
                maxPenetration -= Vector3.Distance(hit.point, exitHit.point);
            else // Wall too thick
                return false;

        }
        return false;
    }

    bool IsShooterCollider(ulong shooterId, Collider collider)
    {
        return playerColliders.TryGetValue(shooterId, out var ownColliders) && ownColliders.Contains(collider);
    }

    // Upon getting input from mouse movement
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

    private void OnHealthChanged(int previousValue, int newValue)
    {
        playerUI.UpdateHealthPoints(newValue);

        if (newValue <= 0)
        {
            onDeath.Invoke();
        }
    }


    private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        Debug.Log("Player name IS " + playerName.Value);
    }

    private void OnPlayerDeath()
    {
        Debug.Log("You're dead");
        ResetPlayerRPC();

        TeleportRandomSpawn();
    }

    // Teleport and make sure that the player does not interpolate between original and spawn location
    private void TeleportRandomSpawn()
    {
        Transform randomSpawnPointTransform = GameManager.Instance.GetRandomSpawnPoint().transform;
        playerNetTransform.Teleport(randomSpawnPointTransform.position, randomSpawnPointTransform.rotation, randomSpawnPointTransform.localScale);
    }


    [Rpc(SendTo.Server)]
    private void ResetPlayerRPC()
    {
        playerHealth.Value = 100;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
}
