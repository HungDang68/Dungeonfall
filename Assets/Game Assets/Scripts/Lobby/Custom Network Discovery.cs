using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    public NetworkManager networkManager;

    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        // Respond to discovery requests only if the server is active
        if (NetworkServer.active)
        {
            return new DiscoveryResponse
            {
                serverAddress = networkManager.networkAddress,
                maxConnections = networkManager.maxConnections,
                currentPlayers = NetworkServer.connections.Count
            };
        }

        // If the server is not active, return null
        return null;
    }

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        // Handle the response from other servers
        Debug.Log($"Discovered server at {response.serverAddress} with {response.currentPlayers}/{response.maxConnections} players.");
        // You can add UI logic here to display discovered servers
    }
}

[System.Serializable]
public class DiscoveryRequest : NetworkMessage
{
    // Add any custom fields if needed
}

[System.Serializable]
public class DiscoveryResponse : NetworkMessage
{
    public string serverAddress;
    public int maxConnections;
    public int currentPlayers;
}