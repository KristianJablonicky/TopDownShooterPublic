using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;

/// <summary>
/// Pretty much taken from:
/// https://youtu.be/fdkvm21Y0xE?list=PL49GwG9Fn8t9vKpEYi_d7NJBxWiUSFX5k
/// </summary>
public class Relay : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    private const int maxPlayers = 4;

    public string JoinCode { get; private set; }

    private async void Awake()
    {
        await Authenticate();
    }

    public void SetJoinCode(string joinCode)
    {
        JoinCode = joinCode;
    }

    private async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateGame()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }

    public async void JoinGame()
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(JoinCode);
        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
        NetworkManager.Singleton.StartClient();
    }
}
