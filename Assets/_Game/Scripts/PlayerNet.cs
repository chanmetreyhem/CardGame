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
     public bool isLocal => isLocalPlayer;
     public bool isOwner => isOwner;
     public SyncList<CardData> hands = new SyncList<CardData>();
     public int sum = 0;
    public Button drawnCardButton;

    private void Awake()
    {
        hands.OnChange += OnHandsChange;
    }

    public override void OnStartClient()
    {
        print($"{netId}: OnStartClient");
        base.OnStartClient();
      
    }

    private void OnHandsChange(SyncList<CardData>.Operation operation, int index, CardData data)
    {
        //print($"hande change {index} | {operation}");

        if (operation == SyncList<CardData>.Operation.OP_ADD)
        {
            // if (isLocalPlayer) print($"card data: {data.Name}");
            if (isClient)
            {
                CardGameController.instance.CloneCardGameObject(data, isLocalPlayer);
            }
           
            if (hands != null)
            {
              
                CardGameController.instance.GetSumOfCardValue(hands.ToList(), out sum, isLocalPlayer);
            }
        }

    }

    private void Start()
    {
        //print($"isLocalPlayer:{isLocalPlayer}|isOwner:{isOwned}|isClient:{isClient}");
        if (!isLocalPlayer) return;
        name = "You";
        drawnCardButton = GameObject.Find("DrawnButton").GetComponent<Button>();
        drawnCardButton.onClick.AddListener(CmdDrawnCard);
        
    }



    [Command]
    public void CmdDrawnCard()
    {
        print($"{netId}: request drawn card");
        CardGameController.instance.DrawnCard(connectionToClient);
        TargetDrawCard();
    }


    [TargetRpc]
    private void TargetDrawCard()
    {
        drawnCardButton.gameObject.SetActive(false);
        print("target");
    }

}
