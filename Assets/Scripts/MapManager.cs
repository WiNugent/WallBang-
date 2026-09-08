using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*PlayerScript[] players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        Debug.Log("activating players");
        foreach (PlayerScript player in players)
        {
            player.gameObject.SetActive(true);
            Debug.Log($"Activated {player.name}");
            player.transform.position = Vector3.up * 3;
        }*/
    }
}
