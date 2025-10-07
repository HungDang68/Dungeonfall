using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;



public class LobbyUIManeger : NetworkBehaviour
{
    public static LobbyUIManeger instance;
    public Transform playerListParent;
    public List<TextMeshProUGUI> playerNameText = new List<TextMeshProUGUI>();
    public List<PlayerHandler> playerHandlers = new List<PlayerHandler>();
    public Button playButton;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"{gameObject.name} was disabled. Re-enabling it.");
            gameObject.SetActive(true);
        }

        playButton.interactable = false;
    }
    public void UpdatePlayerLobbyUI()
    {
        playerNameText.Clear();
        playerHandlers.Clear();

        // Get all player handlers from the network
        PlayerHandler[] allPlayers = FindObjectsByType<PlayerHandler>(FindObjectsSortMode.None);

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("No players found in the lobby.. retrying...");
            StartCoroutine(RetryUpdate());
            return;
        }

        // Sort players: host first, then others
        List<PlayerHandler> orderedPlayers = new List<PlayerHandler>();
        PlayerHandler hostPlayer = null;

        // Find the host player
        foreach (var player in allPlayers)
        {
            if (player.isServer && player.isLocalPlayer)
            {
                hostPlayer = player;
                break;
            }
        }

        // If no local host found, look for any server player
        if (hostPlayer == null)
        {
            foreach (var player in allPlayers)
            {
                if (player.isServer)
                {
                    hostPlayer = player;
                    break;
                }
            }
        }

        // Add host first, then other players
        if (hostPlayer != null)
        {
            orderedPlayers.Add(hostPlayer);
        }

        foreach (var player in allPlayers)
        {
            if (!orderedPlayers.Contains(player))
            {
                orderedPlayers.Add(player);
            }
        }

        // Update UI for each player
        for (int i = 0; i < orderedPlayers.Count; i++)
        {
            if (i >= playerListParent.childCount)
            {
                Debug.LogWarning($"Not enough UI slots for all players. Need: {orderedPlayers.Count}, Have: {playerListParent.childCount}");
                break;
            }

            TextMeshProUGUI txtMesh = playerListParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            PlayerHandler playerHandler = orderedPlayers[i];

            playerHandlers.Add(playerHandler);
            playerNameText.Add(txtMesh);

            // Use the player name from NetworkDiscoveryHUD or generate one
            string playerName = GetPlayerDisplayName(playerHandler, i);
            playerNameText[i].text = playerName;

            // Position the player handler in the UI hierarchy
            playerHandler.transform.SetParent(playerListParent.GetChild(i), false);
        }

        // Clear any unused slots
        for (int i = orderedPlayers.Count; i < playerListParent.childCount; i++)
        {
            TextMeshProUGUI txtMesh = playerListParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            txtMesh.text = "Waiting for player...";
        }

        // Check ready status after UI update
        if (NetworkServer.active)
        {
            CheckAllPlayerReady();
        }
    }
    private string GetPlayerDisplayName(PlayerHandler player, int index)
    {
        // For local player, use the name from NetworkDiscoveryHUD
        if (player.isLocalPlayer && NetworkDiscoveryHUD.instance != null)
        {
            return NetworkDiscoveryHUD.instance.PlayerName;
        }


        // Fallback names based on role and index
        if (player.isServer)
        {
            return $"Host {index + 1}";
        }
        else
        {
            return $"Player {index + 1}";
        }
    }
    public void OnPlayButtonClicked()
    {
        if (NetworkServer.active)
        {
            CustomNetworkManager.singleton.ServerChangeScene("Dungeon");
        }
    }
    public void RegisterPlayer(PlayerHandler player)
    {
        Debug.Log($"Registering player: {player.name}");
        player.transform.SetParent(playerListParent, false);
        UpdatePlayerLobbyUI();
    }
    [Server]
    public void CheckAllPlayerReady()
    {
        Debug.Log("Checking if all players are ready...");
        foreach (var player in playerHandlers)
        {
            if (!player.isReady)
            {
                RpcSetPlayButtonInteractable(false);
                return;
            }
        }
        RpcSetPlayButtonInteractable(true);
    }
    [Server]
    void RpcSetPlayButtonInteractable(bool truthStatus)
    {
        Debug.Log($"Setting play button interactable: {truthStatus}");
        playButton.interactable = truthStatus;
    }
    private IEnumerator RetryUpdate()
    {
        yield return new WaitForSeconds(1);
        UpdatePlayerLobbyUI();
    }
    public void LeaveRoom()
    {
        Debug.Log("LeaveRoom called - Server: " + NetworkServer.active + ", Client: " + NetworkClient.isConnected);

        if (NetworkServer.active)
        {
            // Host is leaving - stop host and disconnect all clients
            Debug.Log("Host is leaving the room - stopping host");
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            // Client is leaving - stop client
            Debug.Log("Client is leaving the room - stopping client");
            NetworkManager.singleton.StopClient();
        }
        else
        {
            Debug.Log("Not connected to any room");
        }

        // Clear lobby UI
        ClearLobbyUI();
    }

    private void ClearLobbyUI()
    {
        playerNameText.Clear();
        playerHandlers.Clear();

        // Clear all player slots
        for (int i = 0; i < playerListParent.childCount; i++)
        {
            TextMeshProUGUI txtMesh = playerListParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            if (txtMesh != null)
            {
                txtMesh.text = "Waiting for player...";
            }
        }

        // playButton.interactable = false;
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Client stopped - cleaning up lobby");
        ClearLobbyUI();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped - cleaning up lobby");
        ClearLobbyUI();
    }
}
