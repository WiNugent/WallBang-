using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private Image crosshair;
    public PlayerScript playerScript;



    private static PlayerUI _instance;
    public static PlayerUI Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            Debug.LogError("Multiple instances of PlayerUI found. Destroying copy.");
        }
        else
        {
            _instance = this;
        }
    }


    // Actual code here

    public void UpdateHealthPoints(int newHealthPoints)
    {
        healthPointsText.text = "HP: " + newHealthPoints.ToString();
    }

    private void Update()
    {
        if (playerScript.IsPlayerInCrosshair()) crosshair.color = Color.red;
        else crosshair.color = Color.white;
    }

}
