using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Prefabs must be registered in NetworkManager
    public GameObject[] characterPrefabs;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Ask client for their choice
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            // Could call a ServerRpc here if client is already a NetworkBehaviour
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestSpawnServerRpc(ulong clientId, int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length) return;

        GameObject prefabToSpawn = characterPrefabs[prefabIndex];
        GameObject playerInstance = Instantiate(prefabToSpawn);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}
