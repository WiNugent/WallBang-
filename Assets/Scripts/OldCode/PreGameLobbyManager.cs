using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class PreGameLobbyManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;

    [SerializeField] private GameObject DebugOptions;
    [SerializeField] private TMP_InputField serverCodeInputField;
    [SerializeField] private Button startGameButton;
    public GameObject scrollContent;

    private ISession currentSession;

    private void Start()
    {
        lobbyNameInputField.text = PlayerPrefs.GetString("Username") + "'s Lobby";
    }

    public void CreatePrivateMatch()
    {
        CreateSession();
    }

    public async Task CreateSession()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SessionOptions option = new SessionOptions().WithRelayNetwork();

        option.MaxPlayers = 10;

        currentSession = await MultiplayerService.Instance.CreateSessionAsync(option);
        Debug.Log(currentSession.Code);

        if (IsOwner)
            startGameButton.gameObject.SetActive(true);

        NetworkManager.Singleton.SceneManager.LoadScene("DM_Maze", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void JoinPrivateMatch()
    {
        if (serverCodeInputField.text == "")
        {
            return;
        }

        JoinSession();
    }

    async Task JoinSession()
    {
        await UnityServices.InitializeAsync(); // Initialize communication with the Unity Network
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Skip authentication step

        currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(serverCodeInputField.text);
    }

    private void OnApplicationQuit()
    {
        ShutDownNetwork();
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy called");
        //ShutDownNetwork();
    }

    private void ShutDownNetwork()
    {
        if (IsHost && currentSession != null)
        {
            currentSession.LeaveAsync();
        }
    }

    //[Rpc(SendTo.Server)]
    public void StartMatch()
    {
        Debug.Log("Starting Match");
        //NetworkManager.Singleton.NetworkConfig
        //NetworkManager.Singleton.SceneManager.LoadScene("DM_Maze", UnityEngine.SceneManagement.LoadSceneMode.Single);
        
    }
}
