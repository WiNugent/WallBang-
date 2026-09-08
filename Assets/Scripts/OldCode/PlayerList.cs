using System.Linq;
using UnityEngine;

// List of players in the pre-game lobby

public class PlayerList : MonoBehaviour
{
    public static PlayerList Instance { get; private set; }

    [SerializeField] private Transform container;
    [SerializeField] private GameObject playerBannerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshScoreboard()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        var activePlayers = FindObjectsByType<PlayerLobbyScript>(FindObjectsSortMode.None);
        var sortedPlayers = activePlayers.OrderByDescending(p => p.PlayerName.ToString()).ToList(); // Consider potential problem here

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            GameObject row = Instantiate(playerBannerPrefab, container);

            PGPlayerBanner bannerUI = row.GetComponent<PGPlayerBanner>();
            bannerUI.setUserData(
                username: sortedPlayers[i].PlayerName.Value.ToString(),
                color: sortedPlayers[i].PlayerColor.Value
                );
        }

    }
}




/*
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class LiveScoreboardUI : MonoBehaviour
{
    public static LiveScoreboardUI Instance { get; private set; }

    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject scoreboardPanel; // Drag your UI panel here

    private void Awake()
    {
        Instance = this;
        scoreboardPanel.SetActive(false); // Hide by default
    }

    private void Update()
    {
        // Classic FPS 'Tab' key to show scoreboard
        if (Input.GetKeyDown(KeyCode.Tab)) scoreboardPanel.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Tab)) scoreboardPanel.SetActive(false);
    }

    public void RefreshScoreboard()
    {
        // Clear old rows
        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }

        // Find all active player instances in the scene
        var activePlayers = FindObjectsByType<NetworkPlayerStats>(FindObjectsSortMode.None);

        // Sort players by kills descending (FPS Standard)
        var sortedPlayers = activePlayers.OrderByDescending(p => p.Kills.Value).ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            GameObject row = Instantiate(rowPrefab, rowContainer);
            
            // Assuming your Prefab row has a helper script to assign strings
            ScoreboardRowUI rowUI = row.GetComponent<ScoreboardRowUI>();
            rowUI.SetRowData(
                rank: (i + 1).ToString(),
                name: sortedPlayers[i].PlayerName.Value.ToString(),
                kills: sortedPlayers[i].Kills.Value.ToString(),
                deaths: sortedPlayers[i].Deaths.Value.ToString()
            );
        }
    }
}

*/