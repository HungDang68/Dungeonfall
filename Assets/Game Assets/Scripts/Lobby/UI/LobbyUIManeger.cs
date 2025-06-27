using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;
using Mirror.BouncyCastle.Asn1.Misc;
using Unity.VisualScripting;


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
        playButton.interactable = false;
    }
    public void UpdatePlayerLobbyUI()
    {
        playerNameText.Clear();
        playerHandlers.Clear();

        var lobby = new CSteamID(SteamLobby.instance.lobbyID);
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);

        CSteamID hostID = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(lobby, "HostAddress")));
        List<CSteamID> orderedMembers = new List<CSteamID>();

        if (memberCount == 0)
        {
            Debug.LogWarning("Lobby has no members.. retrying...");
            StartCoroutine(RetryUpdate());
            return;
        }

        orderedMembers.Add(hostID);

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
            if (memberID != hostID)
            {
                orderedMembers.Add(memberID);
            }
        }
        int j = 0;
        foreach (var member in orderedMembers)
        {
            TextMeshProUGUI txtMesh = playerListParent.GetChild(j).GetChild(0).GetComponent<TextMeshProUGUI>();
            PlayerHandler playerHandler = playerListParent.GetChild(j).GetComponent<PlayerHandler>();

            playerHandlers.Add(playerHandler);
            playerNameText.Add(txtMesh);

            string playerName = SteamFriends.GetFriendPersonaName(member);
            playerNameText[j].text = playerName;
            j++;
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
        player.transform.SetParent(playerListParent, false);
        UpdatePlayerLobbyUI();
    }
    [Server]
    public void CheckAllPlayerReady()
    {
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
    [ClientRpc]
    void RpcSetPlayButtonInteractable(bool truthStatus)
    {
        playButton.interactable = truthStatus;
    }
    private IEnumerator RetryUpdate()
    {
        yield return new WaitForSeconds(1);
        UpdatePlayerLobbyUI();
    }
}
