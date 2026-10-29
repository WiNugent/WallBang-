using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{

    [Header("Game Buttons")]
    [SerializeField] private GameObject createGameButton;
    [SerializeField] private GameObject joinGameButton;
    [Space(15)]
    [SerializeField] private GameObject[] dontDestroy;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField roomCodeInputField;
    [SerializeField] private TMP_Dropdown mapDropdown;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [HideInInspector] public static string playerNameTemp;
    private static string roomCode;

    [Header("Other")]
    public List<SpawnPoint> spawnPoints;


    private bool isSceneLoaded = false;

    private ISession currentSession;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 2. Ensure only one instance exists in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroys duplicates
            return;
        }

        Instance = this;
    }

    public string GetPlayerName()
    {
        return playerNameTemp;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        SceneManager.sceneLoaded += SceneLoaded;

        foreach (GameObject obj in dontDestroy)
        {
            DontDestroyOnLoad(obj);
        }
    }


    public async Task CreateSession()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SessionOptions option = new SessionOptions().WithRelayNetwork();

        option.MaxPlayers = 7;

        currentSession = await MultiplayerService.Instance.CreateSessionAsync(option);
        GUIUtility.systemCopyBuffer = currentSession.Code;
        Debug.Log(currentSession.Code + " Copied to Clipboard");

        NetworkManager.Singleton.SceneManager.LoadScene(mapDropdown.options[mapDropdown.value].text, UnityEngine.SceneManagement.LoadSceneMode.Single);
        
    }

    async Task JoinSession()
    {
        if (roomCodeInputField.text.IsNullOrEmpty())
        {
            Debug.LogError("Join failed: The input field is empty");
            return;
        }

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"Attempting to join game (Room Code: {roomCodeInputField.text}");
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(roomCodeInputField.text);

            Debug.Log("Success! you connected!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to join session");
            Debug.LogError(ex.Message);
        }


    }

    public void CreatePrivateMatch()
    {
        CreateSession();
    }

    public void JoinPrivateMatch()
    {
        JoinSession();
    }

    public FixedString32Bytes GetSessionCode() // 
    {
        return currentSession.Code;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
        SpawnPlayer(clientId);
        
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneLoaded = true;
        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (ulong clientId in clientIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!isSceneLoaded || !IsServer)
            return;

        GameObject playerInstance = Instantiate(
            playerPrefab,
            Vector3.up,
            Quaternion.identity
            );

        var netObj = playerInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
        
        
    }


    public void AddSpawnpointToList(SpawnPoint spawnPoint)
    {
        spawnPoints.Add(spawnPoint);
    }

    public SpawnPoint GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public string GetRoomCode()
    {
        return currentSession.Code;
    }

}
