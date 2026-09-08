using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

/*
 * Script used by player upon joining session (In lobby)
 * NetPlayerInfo prefab
 */

public class PlayerLobbyScript : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Color> PlayerColor = new(Color.black, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            PlayerName.Value = new FixedString32Bytes(PlayerPrefs.GetString("Username"));
            PlayerColor.Value = Random.ColorHSV();
            Debug.Log(PlayerName.Value.ToString() + " has joined");
        }

        PlayerList.Instance.RefreshScoreboard();

        PlayerName.OnValueChanged += OnStatsChanged;
        PlayerColor.OnValueChanged += OnStatsChanged;
        Debug.Log("Triggered");

    }
    private void OnStatsChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue) => PlayerList.Instance.RefreshScoreboard();
    private void OnStatsChanged(Color previousValue, Color newValue) => PlayerList.Instance.RefreshScoreboard();

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerName.OnValueChanged -= OnStatsChanged;
    }

    [Rpc(SendTo.Server)]
    public void ChangeRandomUsernameRPC()
    {
        if (IsOwner)
        {
            string newUsername = "Player#" + Random.Range(0, 9999).ToString();
            Debug.Log(PlayerPrefs.GetString("Username") + " -> " + newUsername);
            PlayerPrefs.SetString("Username", newUsername);
            PlayerName.Value = new FixedString32Bytes(newUsername);
        }
    }

}

/*
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerStats : NetworkBehaviour
{
    // NetworkVariables sync automatically from Server to all Clients
    public NetworkVariable<FixedString32Bytes> PlayerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Kills = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Deaths = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Set local player name on spawn (e.g., from account settings)
            PlayerName.Value = "Player_" + Random.Range(100, 999);
        }

        // Trigger UI refresh when any player's stats change
        Kills.OnValueChanged += OnStatsChanged;
        Deaths.OnValueChanged += OnStatsChanged;
        PlayerName.OnValueChanged += OnStatsChanged;

        LiveScoreboardUI.Instance.RefreshScoreboard();
    }

    private void OnStatsChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue) => LiveScoreboardUI.Instance.RefreshScoreboard();
    private void OnStatsChanged(int previousValue, int newValue) => LiveScoreboardUI.Instance.RefreshScoreboard();

    // Call this ONLY on the server when a kill happens
    [ServerRpc(RequireOwnership = false)]
    public void AwardKillServerRpc()
    {
        if (!IsServer) return;
        Kills.Value++;
    }

    public override void OnNetworkDespawn()
    {
        Kills.OnValueChanged -= OnStatsChanged;
        Deaths.OnValueChanged -= OnStatsChanged;
        PlayerName.OnValueChanged -= OnStatsChanged;
        if (LiveScoreboardUI.Instance != null) LiveScoreboardUI.Instance.RefreshScoreboard();
    }
}

*/