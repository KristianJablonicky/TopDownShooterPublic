using Unity.Netcode;
using UnityEngine;

public class NetworkHeroSpawner : MonoBehaviour
{
    public static void StartHost()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetPayload();
        NetworkManager.Singleton.StartHost();
    }

    public static void StartClient()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetPayload();
        NetworkManager.Singleton.StartClient();
    }

    private static byte[] GetPayload()
    {
        byte[] payload = new byte[1];
        payload[0] = (byte)DataStorage.Instance.GetInt(DataKeyInt.PickedHero);
        return payload;
    }


    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        var data = request.Payload;
        var prefabIndex = data.Length > 0 ? data[0] : 0;

        var prefabList = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

        if (prefabIndex < 0 || prefabIndex >= prefabList.Count)
        {
            Debug.LogError($"prefabIndex of value {prefabIndex} not supported.");
            prefabIndex = 0;
        }

        var playerPrefab = prefabList[prefabIndex].Prefab;
        response.PlayerPrefabHash = playerPrefab.GetComponent<NetworkObject>().PrefabIdHash;
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = Vector2.zero;

        //var mediator = playerPrefab.GetComponent<CharacterMediator>();
    }
}
