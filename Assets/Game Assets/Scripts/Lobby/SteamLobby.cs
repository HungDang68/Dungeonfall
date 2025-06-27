using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
using Steamworks;
using Mirror.BouncyCastle.Bcpg.OpenPgp;
using System;
using Unity.VisualScripting.Antlr3.Runtime;

public class SteamLobby : NetworkBehaviour
{
    public static SteamLobby instance;
    public GameObject hostButton = null;
    public ulong lobbyID;
    public NetworkManager networkManager;
    public PanelSwapper panelSwapper;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    private const string HostAddressKey = "HostAddress";

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
        networkManager = FindAnyObjectByType<NetworkManager>();

        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam is not initalized. Make sure to run this game in the steam environment");
            return;
        }
        panelSwapper.gameObject.SetActive(true);
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }
    public void HostSinglePlayer()
    {
        networkManager.StartHost();

        CustomNetworkManager.singleton.ServerChangeScene("Dungeon");
    }
    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }
    void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        if (callback.m_ulSteamIDLobby != lobbyID) return;

        EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
        Debug.Log($"Lobby ChatUpdate: {stateChange}");

        bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
        stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
        stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
        stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
        stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);

        if (shouldUpdate)
        {
            StartCoroutine(DelayedNameUpdate(0.5f));
            LobbyUIManeger.instance?.CheckAllPlayerReady();
        }
    }

    private IEnumerator DelayedNameUpdate(float delay)
    {
        if (LobbyUIManeger.instance == null)
        {
            Debug.Log("Looby UI Manager instance is null");
            yield break;
        }

        yield return new WaitForSeconds(delay);
        LobbyUIManeger.instance?.UpdatePlayerLobbyUI();
    }
    public void LeaveLobby()
    {
        CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
        CSteamID me = SteamUser.GetSteamID();
        var lobby = new CSteamID(lobbyID);
        List<CSteamID> members = new List<CSteamID>();

        int count = SteamMatchmaking.GetNumLobbyMembers(lobby);

        for (int i = 0; i < count; i++)
        {
            members.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
        }
        if (lobbyID != 0)
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
            lobbyID = 0;
        }
        if (NetworkServer.active && currentOwner == me)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        panelSwapper.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        panelSwapper.SwapPanel("MainPanel");
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active)
        {
            Debug.Log("Already in a lobby as a host");
            return;
        }
        lobbyID = callback.m_ulSteamIDLobby;
        string _hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        networkManager.networkAddress = _hostAddress;
        Debug.Log("Entered lobby: " + callback.m_ulSteamIDLobby);
        networkManager.StartClient();
        panelSwapper.SwapPanel("LobbyPanel");
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Join request received for lobby " + callback.m_steamIDLobby);

        if (NetworkClient.isConnected || NetworkClient.active)
        {
            Debug.LogError("NetworkClient is active or connected");
            NetworkManager.singleton.StopClient();
            NetworkClient.Shutdown();
        }
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }


    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log("Failed to create lobby" + callback.m_eResult);
            return;
        }

        Debug.Log("Lobby created, ID: " + callback.m_ulSteamIDLobby);
        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        lobbyID = callback.m_ulSteamIDLobby;
    }


}

