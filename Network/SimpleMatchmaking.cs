using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Playmode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;


/// <summary>
/// Stolen from here:
/// https://gist.github.com/Matthew-J-Spencer/a5ab1fb5a50465e300ea39d7cde85006
/// </summary>

public class SimpleMatchmaking : MonoBehaviour
{
    [SerializeField] private UnityTransport _transport;

    [Header("References")]
    [SerializeField] private GameObject joiningAMatchOverlay;
    [SerializeField] private CanvasGroup joiningCanvasGroup;
    [SerializeField] private GameObject networkTestButtons;
    [SerializeField] private float fadeAnimationDuration = 1f;

    private Lobby _connectedLobby;
    private QueryResponse _lobbies;
    private const string JoinCodeKey = "j";
    private string _playerId;
    
    private async void Awake()
    {
#if UNITY_EDITOR
        HideJoiningOverlay(true);
        var tags = CurrentPlayer.ReadOnlyTags();

        if (CurrentPlayer.IsMainEditor)
        {
            await Task.Delay(200);
            NetworkHeroSpawner.StartHost();
        }
        else if (tags.Contains("Client"))
        {
            if (int.TryParse(tags[1], out int parsed))
            {
                await Task.Delay(500 * parsed);
            }
            else
            {
                Debug.LogWarning($"Could not parse {tags[1]} into int");
            }
            
            NetworkHeroSpawner.StartClient();
        }
#else
        Destroy(networkTestButtons);
        joiningCanvasGroup.alpha = 1f;
        CreateOrJoinLobby();
#endif
    }

    public async void CreateOrJoinLobby()
    {
        await Authenticate();

        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
        HideJoiningOverlay(false);
    }

    private void HideJoiningOverlay(bool instant)
    {
        if (instant)
        {
            Destroy(joiningAMatchOverlay);
        }
        else
        {
            Tweener.Tween(this, 1f, 0f, fadeAnimationDuration, TweenStyle.quadratic,
                (value) => joiningCanvasGroup.alpha = value,
                () => Destroy(joiningAMatchOverlay)
            );
        }
    }

    private async Task Authenticate()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            var options = new InitializationOptions();

#if UNITY_EDITOR
            var uniqueString = DateTime.UtcNow.ToString("ddHHmmssfff");
            options.SetProfile(uniqueString);
#endif

            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _playerId = AuthenticationService.Instance.PlayerId;
    }
    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            // Attempt to join a lobby in progress
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            // If we found one, grab the relay allocation details
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            // Set the details to the transform
            SetTransformAsClient(a);

            // Join the game room as a client
            //NetworkManager.Singleton.StartClient();
            NetworkHeroSpawner.StartClient();

            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No lobbies available via quick join\n{e}");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            // Create a relay allocation and generate a join code to share with the lobby
            var a = await RelayService.Instance.CreateAllocationAsync(Constants.maxPlayerCount);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            // Create a lobby, adding the relay join code to the lobby data
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync("Useless Lobby Name", Constants.maxPlayerCount, options);

            // Send a heartbeat every 15 seconds to keep the room alive
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            // Set the game room to use the relay allocation
            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            // Start the room. I'm doing this immediately, but maybe you want to wait for the lobby to fill up
            //NetworkManager.Singleton.StartHost();
            NetworkHeroSpawner.StartHost();

            Debug.Log($"Join code: {joinCode}");

            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogFormat($"Failed creating a lobby\n{e}");
            return null;
        }
    }

    private void SetTransformAsClient(JoinAllocation a)
    {
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private async void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
            // todo: Add a check to see if you're host
            if (_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId) await LobbyService.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else await LobbyService.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }
}
