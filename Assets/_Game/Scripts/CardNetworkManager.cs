using Mirror;
using System.Linq;
using UnityEngine;

public class CardNetworkManager : NetworkManager
{

    [SerializeField] private GameObject menuPanel;

  
    public override void OnStartServer()
    {
        GameManager.Instance.index = 1;
        print($"{nameof(CardNetworkManager)} : on start server");
        menuPanel.SetActive(false);
        base.OnStartServer();

        CardGameController.instance.OnStartServer();
    }

    public override void OnStopServer()
    {
        print($"{nameof(CardNetworkManager)} : on stop server");


        menuPanel.SetActive(true);
        base.OnStopServer();

        CardGameController.instance.OnStopServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
       
        Debug.Log(nameof(CardNetworkManager) + ": Server Add Player ! : ConnectionID: " + conn.connectionId );
        GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);
        var playerNet = player.GetComponent<PlayerNet>();


        CardGameController.instance.playersDic.Add(conn.connectionId.ToString(), playerNet);
        playerNet.Index = CardGameController.instance.playersDic.Keys.ToList().IndexOf(conn.connectionId.ToString());
        CardGameController.instance.OnServerAddPlayer(conn);
      

    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        print($"{nameof(CardNetworkManager)} : on server connect client: ${conn.connectionId}");
        CardGameController.instance.OnServerConnect(conn);
        base.OnServerConnect(conn);
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        print($"{nameof(CardNetworkManager)} : on server disconnect client: ${conn.connectionId}");
        CardGameController.instance.OnServerDisconnect(conn);
        base.OnServerDisconnect(conn);
    }


    public override void OnClientConnect()
    {
        print($"{nameof(CardNetworkManager)} : on client connect");
        CardGameController.instance.OnClientConnect();
      
        menuPanel.SetActive(false);
        
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        print($"{nameof(CardNetworkManager)} : on client disconnect");
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
