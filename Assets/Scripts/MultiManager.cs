using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiManager : NetworkBehaviour
{
    // Start is called before the first frame update

    public async void Login_Async()
    {
        try
        {
            AuthenticationService.Login();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void CreateLobby()
    {
        try
        {
            MatchmakingService.LobbyData data = new MatchmakingService.LobbyData();
            data.MaxPlayers = 2;
            data.Difficulty = 1;
            data.Type = 0;
            data.Name = "player";
            await MatchmakingService.CreateLobbyWithAllocation(data);

            // Starting the host immediately will keep the relay server alive
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void OnLobbySelected(string id)
    {
        try
        {
            await MatchmakingService.JoinLobbyWithAllocation(id);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
