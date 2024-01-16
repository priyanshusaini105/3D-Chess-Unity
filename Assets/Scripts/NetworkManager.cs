using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Lobby lobby;
    void Start()
    {

        Debug.Log("Room Id" + lobby.roomId);    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   

    
}
