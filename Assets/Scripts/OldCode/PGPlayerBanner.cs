using TMPro;
using UnityEngine;
using UnityEngine.UI;


// Configure player info for the banner
public class PGPlayerBanner : MonoBehaviour
{
    [SerializeField] TMP_Text usernameField;
    [SerializeField] Image image;
    
    public void setUserData(string username, Color color)
    {
        usernameField.text = username;
        image.color = color;

    }
}
