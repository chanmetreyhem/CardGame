using Mirror;
using System;
using UnityEngine;

public class CardNetworkManager : NetworkManager
{

    [SerializeField] private GameObject menuPanel;

  
    public override void OnStartServer()
    {
        //Debug.Log("Server Start !");
      
        menuPanel.SetActive(false);
        base.OnStartServer();

        CardGameController.instance.OnStartServer();
    }

    public override void OnStopServer()
    {
        //Debug.Log("Server Stop !");
        
       
        menuPanel.SetActive(true);
        base.OnStopServer();

        CardGameController.instance.OnStopServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
       

       // Debug.Log("Server Add Player !");
        GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        if(CardGameController.instance.playerOne == null)
        {
            CardGameController.instance.playerOne = player.GetComponent<PlayerNet>();
        }
        else
        {
            CardGameController.instance.playerTwo = player.GetComponent<PlayerNet>();
        }
        NetworkServer.AddPlayerForConnection(conn, player);

        CardGameController.instance.OnServerAddPlayer(conn);
      

    }


    public override void OnClientConnect()
    {
        //print("on client connect ");
        CardGameController.instance.OnClientConnect();
        menuPanel.SetActive(false);
        
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        //print("on client disconnected");
        CardGameController.instance.OnClientDisconnect();
        menuPanel.SetActive(true);
      
        base.OnClientDisconnect();


        
    }

   
}


public static class Extension
{
    public static void WriteCard(this NetworkWriter writer, Card card)
    {
        writer.WriteString(card.Name);
        writer.WriteInt(card.value);
    
    }
}
