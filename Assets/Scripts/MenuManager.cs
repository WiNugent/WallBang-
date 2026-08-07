using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField UserInputField;

    private void Start()
    {
        // Ensure player always has a username
        string currentUsername = PlayerPrefs.GetString("Username");
        if (currentUsername == null || currentUsername == "")
        {
            string randomizedUsername = "Player#" + Random.Range(0, 9999).ToString("D4");
            UserInputField.text = randomizedUsername;
            PlayerPrefs.SetString("Username", randomizedUsername);
        }
        else
        {
            UserInputField.text = PlayerPrefs.GetString("Username");
        }
    }

    public void UsernameSave()
    {
        if (UserInputField.text != null && UserInputField.text.Length > 0)
        {
            PlayerPrefs.SetString("Username", UserInputField.text);
            Debug.Log("Saved Username");
        }
        else
        {
            UserInputField.text = PlayerPrefs.GetString("Username");
            Debug.Log("Invalid Username");
        }
    }

    public void CreateLobby()
    {

    }
}
