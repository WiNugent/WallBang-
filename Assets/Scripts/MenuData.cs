using TMPro;
using UnityEngine;

public class MenuData : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField usernameInputField;



    public static MenuData Instance;

    public static string playerName;



    private void Awake()
    {
        // 2. Ensure only one instance exists in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroys duplicates
            return;
        }

        Instance = this;

        usernameInputField.onEndEdit.AddListener(HandleEndEdit);

    }

    private void HandleEndEdit(string newUsername)
    {
        playerName = newUsername;
        Debug.Log("Player name set to " + newUsername);
    }

}
