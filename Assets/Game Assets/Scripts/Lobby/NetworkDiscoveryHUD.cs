using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using TMPro;
public class NetworkDiscoveryHUD : MonoBehaviour
{
    public static NetworkDiscoveryHUD instance;
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button discoverButton;
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomListItemPrefab;
    public string PlayerName => string.IsNullOrEmpty(playerNameInput.text) ? "Player" : playerNameInput.text;
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

    private NetworkDiscovery networkDiscovery;
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        networkDiscovery = FindFirstObjectByType<NetworkDiscovery>();

        // hostButton.onClick.AddListener(OnHostButtonClicked);
        discoverButton.onClick.AddListener(OnDiscoverButtonClicked);

        // Subscribe to network discovery events
        networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
    }

    public void OnHostButtonClicked()
    {    // Check if the server or client is already running
        if (NetworkServer.active || NetworkClient.isConnected)
        {
            Debug.LogWarning("Server or Client already started.");
            return;
        }
        hostButton.interactable = false;
        CustomNetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
        hostButton.interactable = true;

        // // Switch to lobby panel like SteamLobby does
        // if (panelSwapper != null)
        //     panelSwapper.SwapPanel(lobbyPanelName);
    }

    public void OnDiscoverButtonClicked()
    {
        ClearRoomList();
        networkDiscovery.StartDiscovery();
    }

    private void OnDiscoveredServer(ServerResponse response)
    {
        // This gets called when a server is discovered
        string serverName = $"Room {response.EndPoint.Address}";
        AddRoom(serverName, response.uri.Host);
    }

    public void AddRoom(string roomName, string address)
    {
        if (roomListItems.ContainsKey(address))
        {
            Debug.Log($"Room with address {address} already exists in the list.");
            return;
        }

        GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);

        // Set room name text using TextMeshProUGUI
        TextMeshProUGUI roomNameText = roomItem.GetComponentInChildren<TextMeshProUGUI>();
        if (roomNameText != null)
        {
            roomNameText.text = roomName;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in roomListItemPrefab.");
        }

        // Add join functionality
        roomItem.GetComponentInChildren<Button>().onClick.AddListener(() => JoinRoom(address));
        // roomItem.GetComponentInChildren<Button>().onClick.AddListener(() => MenuManager.instance.SwapPanel("LobbyPanel"));
        roomListItems[address] = roomItem;
    }

    public void RemoveRoom(string address)
    {
        if (roomListItems.TryGetValue(address, out GameObject roomItem))
        {
            Destroy(roomItem);
            roomListItems.Remove(address);
        }
    }

    private void JoinRoom(string address)
    {
        // Set network address and start client like in SteamLobby
        CustomNetworkManager.singleton.networkAddress = address;
        CustomNetworkManager.singleton.StartClient();

        // // Switch to lobby panel when joining, similar to SteamLobby behavior
        // if (panelSwapper != null)
        //     panelSwapper.SwapPanel(lobbyPanelName);
    }

    private void ClearRoomList()
    {
        foreach (var roomItem in roomListItems.Values)
        {
            Destroy(roomItem);
        }
        roomListItems.Clear();
    }

    // Add leave lobby functionality similar to SteamLobby
    public void LeaveLobby()
    {
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        // // Return to main panel
        // if (panelSwapper != null)
        //     panelSwapper.SwapPanel(mainPanelName);

        // Clear room list when leaving
        ClearRoomList();
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (networkDiscovery != null)
            networkDiscovery.OnServerFound.RemoveListener(OnDiscoveredServer);
    }
}