using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;



public class LobbyManager : MonoBehaviour
{
    // Start is called before the first frame update
    public string roomId;
    //[SerializeField] public NetworkManager networkManager; 
    public bool IsLobbyHost = false;
    public int playerNum = 1;
    public GameObject dialogBox;
    private float lobbyUpdateTimer;
    private float heartBeatTimer;
    private Unity.Services.Lobbies.Models.Lobby joinedLobby;
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLobbyPollForUpdates();
    }
    private async void HandleHeartBeatSync()
    {
        if(joinedLobby!=null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer<0f)
            {
                float heartBeatTimerMax = 15;
                heartBeatTimer = heartBeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }

        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 3.1f;
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                playerNum = 0;
                foreach (Player player in joinedLobby.Players)
                {
                    playerNum += 1;
                }
                Debug.Log("Player COunt " + playerNum);

                if (joinedLobby.Data["KEY_START_GAME"].Value != "0")
                {
                    if (!IsLobbyHost)
                   {
                        Debug.Log("Relay Joined " + joinedLobby.Data["KEY_START_GAME"].Value);
                        JoinRelay(joinedLobby.Data["KEY_START_GAME"].Value);
                    }
                    joinedLobby = null;
                }

            }
        }
    }

    public async void CreateLobby()
    {
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsLocked = false,
            Data = new Dictionary<string, DataObject>
        {
            {"KEY_START_GAME" , new DataObject(DataObject.VisibilityOptions.Member , "0") }
        }
        };
        string lobbyName = "My lobby";
        int maxPlayers = 2;

        Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        joinedLobby = lobby;
        Debug.Log("Lobby Created with Id " + lobby.LobbyCode);
        roomId = lobby.LobbyCode;
        //SceneManager.LoadScene(1);
    }
    public void SpawnGameObject()
    {
        dialogBox.SetActive(true);
    }
    public async void JoinRoom(string s)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(s);
            Debug.Log(s + " Lobby Joined");
            foreach (Player player in joinedLobby.Players)
            {
                Debug.Log(player.Id);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);

            Debug.Log("allocation ID: " + allocation.AllocationId.ToString());

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            Debug.Log(relayServerData.ConnectionData);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address);

            NetworkManager.Singleton.StartHost();

            IsLobbyHost = true;
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining with joincode " + joinCode);
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log("allocation ID: " + joinAllocation.AllocationId.ToString() + "Join Code: " + joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            Debug.Log(relayServerData.ConnectionData);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
       // if (!IsLobbyHost())
      //  {

      //  }
    }
}
