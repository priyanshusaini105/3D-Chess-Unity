using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using UnityEngine;


public class Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    public string roomId;
    public int playerNum = 1;
    public GameObject dialogBox;
    private float lobbyUpdateTimer;
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
        
    }

    private async void HandleLobbyPollForUpdates()
    {
        if(joinedLobby!=null)
        {
            lobbyUpdateTimer -= Time.deltatime;
            if(lobbyUpdateTimer<0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                

                if(joinedLobby.Data[Key_Start_Game].Value !="0")
                {
                    if(!IsLobbyHost())
                    {
                        //join the Relay
                    }
                    joinedLobby = null;
                }

            }
        }   
    }

    public async void CreateLobby()
    {
        CreateLobbyOptions options = new CreateLobbyOptions {
            IsLocked = true,
        Data = new Dictionary<string , DataObject>
        {
            {KEY_START_GAME , new DataObject(DataObject.VisibilityOptions.Member , "0") }
        }
    };
        string lobbyName = "My lobby";
        int maxPlayers = 2;
        options.
        Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers , options);
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
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "dlts");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining with joincode " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dlts");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
        if(!IsLobbyHost)
        {
            try
        }
    }
}
