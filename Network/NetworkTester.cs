using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkTester : MonoBehaviour
{
    [SerializeField] private string hostAddress = "127.0.0.1";
    [SerializeField] private ushort port = 7777;
    [SerializeField] private bool autoStart = true;
    /*
    private async void Start()
    {
        if (!autoStart) return;
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("No NetworkManager found in the scene!");
            return;
        }

        var transport = nm.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = hostAddress;

        // Try client first (don’t assign port for client!)
        nm.StartClient();

        // Wait for connection
        await Task.Delay(1000);

        if (!nm.IsConnectedClient)
        {
            Debug.Log("No host detected — restarting as Host.");
            nm.Shutdown();

            // Small delay to release socket
            await Task.Delay(100);

            transport.ConnectionData.Address = hostAddress;
            transport.ConnectionData.Port = port; // only needed for host
            nm.StartHost();
        }
        else
        {
            Debug.Log("Connected to existing host — running as Client.");
        }
    }
    */
    public void StartHost() => NetworkHeroSpawner.StartHost();

    public void StartClient() => NetworkHeroSpawner.StartClient();
}