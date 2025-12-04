using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class PlayerNet : NetworkBehaviour
{
    [SerializeField] public string connId;
     public string Name;
     public bool isLocal => isLocalPlayer;
    public bool isOwner => isOwner;
    public SyncList<CardData> hands = new SyncList<CardData>();
    public int sum = 0;
    public Button drawnCardButton;
    private float defaultZRotate = 30;
    private float defaultXPos = -100;
    private float middleCardYPos = 82f;
    private float localCardScale = 1.5f;
   // private public GameObject otherPlayerParent;
   // public int localIndex = 0;
    [SyncVar] public int Index = 0;

    private void Awake()
    {
        Name = "player" +UnityEngine. Random.Range(0, 1000);
        hands.OnChange += OnHandsChange;
    }

    public override void OnStartClient()
    {
        print($"netID: {netId}|conID: {connectionToClient}|conToServer: {connectionToServer}: OnStartClient");

        //; CardGameController.instance.localPlayerIndex = ;

        LocalIndexServer();
        base.OnStartClient();
      
    }
    public override void OnStartServer()
    {
        print($"netID: {netId}|conID: {connectionToClient}|conToServer: {connectionToServer}: OnStartServer");
      
        base.OnStartServer();
    }
   
    [ServerCallback]
    public void LocalIndexServer()
    {
        // we can get connectionToClient only on server so we need to call on server and pass to client
        string id = connectionToClient.connectionId.ToString();
        SetLocalIndexClientRpc(id);
    }

    [ClientRpc]
    public void SetLocalIndexClientRpc(string id)
    {
        // set all playerNet localIndex (get from playerDic in GameController) in there  own device 
       // print("run:"+id);
        //CardGameController.instance.GetLocalPlayerIndex(id, out localIndex);
        if (isLocalPlayer)
        {
            // set local index in GameController to short card location
            CardGameController.instance.SetLocalPlayerIndex(id);
        }
    }
     
    public override void OnStopClient()
    {
        //print($"netID: {netId}|conID: {connectionToClient} {connectionToServer}: OnStopClient");
        base.OnStopClient();
    }

    [TargetRpc]
    public void SetLocalId(ulong id)
    {
        CardGameController.instance.localPlayerIndex = (int)id;
    }

    private void OnHandsChange(SyncList<CardData>.Operation operation, int index, CardData data)
    {
        //print($"hand change {index} | {operation}");
        
        Vector2 cardPos = new Vector2(defaultXPos,index == 1 && isLocal ? middleCardYPos : 0);

        if (operation == SyncList<CardData>.Operation.OP_ADD)
        {
            // if (isLocalPlayer) print($"card data: {data.Name}");
            if (isClient)
            {
                CardGameController.instance.CloneCardGameObject(data, isLocalPlayer,cardPos, localCardScale,defaultZRotate,Index);
                defaultXPos += 100;
                defaultZRotate -= 30;
            }
           
            if (hands != null)
            {
              
                CardGameController.instance.GetSumOfCardValue(hands.ToList(), out sum, isLocalPlayer);
            }
        }
        else if (operation == SyncList<CardData>.Operation.OP_CLEAR)
        {

           // Debug.Log("ClearCard");
            defaultZRotate = 30;
            defaultXPos = -100;
            if (!drawnCardButton) return;
            drawnCardButton.gameObject.SetActive(true);
        }

    }

    public override void OnStartLocalPlayer()
    {
        name = "You";
        drawnCardButton = GameObject.Find("DrawnButton").GetComponent<Button>();
        drawnCardButton.onClick.AddListener(CmdDrawnCard);
    }   
  



    [Command]
    public void CmdDrawnCard()
    {
        print($"{netId}: request drawn card");
        CardGameController.instance.DrawnCard(connectionToClient.connectionId.ToString());
        TargetDrawCard();
    }


    [TargetRpc]
    private void TargetDrawCard()
    {
        drawnCardButton.gameObject.SetActive(false);
        //print("target");
    }

}
