using TMPro;
using UnityEngine;

public class PreGameLobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Start()
    {
        lobbyNameInputField.text = PlayerPrefs.GetString("Username") + "'s Lobby";
    }
}
