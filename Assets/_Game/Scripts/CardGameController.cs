
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;




public enum GameState : int
{
    NONE = 0,
    STATED,DEALED,CACULATE,CLEAR,FINISHED
}
public class CardGameController : NetworkBehaviour
{
    
    public static CardGameController instance;
    [InspectorName("SYNC PLAYER DICTIONARY")]
    public SyncDictionary<string, PlayerNet> playersDic = new SyncDictionary<string, PlayerNet>();

    [SerializeField] public List<Sprite> cardsSprite = new List<Sprite>();

     private List<Card> decks = new List<Card>();


    [Header("CARD PREFAB")]
    [SerializeField] private GameObject cardPrefab;

    [Header("LOCATION OF CARD")]
    [SerializeField] private Transform decksPos;
    [SerializeField] private GameObject localPlayerPlayerCardPlace;
    [SerializeField] private GameObject otherPlayerPlayerCardPlace;
    [SerializeField] public List<GameObject> parentPlaces = new List<GameObject>();
    [Header("UI")]
    [Header("TEXT ELEMENT")]
    [SerializeField] private Text localSumOfCardValueText;
    [SerializeField] private Text otherSumOfCardValueText;
    [SerializeField] private Text finalText;
    [Header("BUTTON ELEMENT")]
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button calculateButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button listPlayerButton;
    [Header("Panel")]
    [SerializeField] private GameObject loadingPanel;
    public void SetLoading(bool loading,string message = "")
    {
        loadingPanel.SetActive(loading);

        loadingPanel.GetComponentInChildren<Text>().text = message;
    }
    private void Awake()
    {
       

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }


      

        
    }


    #region NetworkEvent
    public void OnServerConnect(NetworkConnectionToClient conn)
    {

        //// 3. Get all connected player manager instances
        //// NetworkServer.connections.Values gives us all active server connections
        //var players = NetworkServer.connections.Values
        //    .Select(conn => conn.identity.GetComponent<PlayerManager>())
        //    .ToList();
    
    }
    public void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        int indexOf = playersDic.Keys.ToList().IndexOf(conn.connectionId.ToString());
        print("index: " +  indexOf);
        playersDic.Remove(conn.connectionId.ToString());
      
    }
    public void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

        int playerCount = NetworkManager.singleton.numPlayers;
        //Debug.Log($"{nameof(CardGameController)}: OnServerAddPlayer| PlayerCount:{playerCount}");
        if (playerCount == 3)
        {
            StartCoroutine(DealCardCoroutine());
        }
    }
    public void OnClientConnect()
    {
        Debug.Log($"{nameof(CardGameController)}: OnClientConnect");
        calculateButton.gameObject.SetActive(isServer);
        clearButton.gameObject.SetActive(isServer);
        shuffleButton.gameObject.SetActive(isServer);
    }
    public void OnClientDisconnect() {
        //Debug.Log($"{nameof(CardGameController)}: OnDisconnect :${connectionToClient.ToString()}");
    }
    #endregion


    public int localPlayerIndex = 0;

    public void SetLocalPlayerIndex(string conn)
    {
        localPlayerIndex = playersDic.Keys.ToList().IndexOf(conn);
       
    }
    public void GetLocalPlayerIndex(string conn,out int index)
    {
        index = playersDic.Keys.ToList().IndexOf(conn);  
    }

    [ServerCallback]
    public void ListPlayer()
    {
        print($"Count: {playersDic.Count}");
        var message = "";
        foreach (var player in playersDic)
        {
            message +=   $"plauer : {player.Key}\n";
        }
        print(message);
      
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        calculateButton.gameObject.SetActive(false);
        clearButton.gameObject.SetActive(false);
        shuffleButton.gameObject.SetActive(false);
        listPlayerButton.onClick.AddListener(ListPlayer);
        shuffleButton.onClick.AddListener(() =>
        {
            StartCoroutine(DealCardCoroutine());
        });
        calculateButton.onClick.AddListener(() =>
        {
            CalculateServer();
        });


        clearButton.onClick.AddListener(() =>
        {
            RpcClearCard();
        });
    }

   // [ClientRpc]
    private void RpcClearCard()
    {
        clearButton.gameObject.SetActive(false);
        shuffleButton.gameObject.SetActive(true);
        var children = new List<GameObject>();
        for (int i = 0; i < otherPlayerPlayerCardPlace.transform.childCount; i++)
        {
            children.Add(otherPlayerPlayerCardPlace.transform.GetChild(i).gameObject);
            children.Add(localPlayerPlayerCardPlace.transform.GetChild(i).gameObject);
        }
        ShowCard(children, true);
        finalText.text = localSumOfCardValueText.text = otherSumOfCardValueText.text = "";
    }

    IEnumerator DealCardCoroutine()
    {
        ShowLoadingClientRpc();

        yield return new WaitForSeconds(1f);

        HideLoadingClientRpc();

        // server deal cards
        DealCard();
    }

    //[ClientRpc]
    private void ShowLoadingClientRpc()
    {
        SetLoading(true, "start deal card");
    }

    [ClientRpc]
    private void HideLoadingClientRpc()
    {
        SetLoading(false);
    }
    [ServerCallback]
    private void DealCard()
    {
        GenerateCardFromCardSprite();
        StartCoroutine(ShuffleCard());

        shuffleButton.gameObject.SetActive(false);
        calculateButton.gameObject.SetActive(true);
    }

    [ServerCallback]
    private void CalculateServer()
    {

        
    }
   

    int maxCard = 4;
    int maxCardPerPlayer = 2;
    private IEnumerator ShuffleCard()
    {
          
        for (int i = 0; i < maxCardPerPlayer; i++){
            foreach (var player in playersDic)
            {
                var card = decks[0];
                player.Value.hands.Add(new CardData() { Name = decks[i].Name, value = decks[i].value });
                decks.Remove(card);
                yield return new WaitForSeconds(0.5f);
            
            }
           
        }
        
    }

    [ServerCallback]
    public void DrawnCard(string conn)
    {
        var card = decks[0];
        var data = new CardData() { Name = card.Name, value = card.value };

        //if (conn == playerOne.connectionToClient) playerOne.hands.Add(data); else playerTwo.hands.Add(data);
        playersDic[conn.ToString()]?.hands.Add(data);

        decks.Remove(card);
        
    }

   

   
   
    private void GenerateCardFromCardSprite()
    {
       // playerOne.hands.Clear();
      //  playerTwo.hands.Clear();
        decks.Clear();
        cardsSprite.ForEach((s) =>
        {
            string name = s.name;
            Card card = new Card(s);
            decks.Add(card);
            ShuffleList(decks);
        });
    }

    private void  ShuffleList<T>(List<T> list)
    {
        System.Random rand = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            // Swap items
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }



    public void CloneCardGameObject(CardData card, bool isLocalPlayer, Vector2 pos, float localScale, float angleZ,int playerID)
    {
        int localTransformIndex = (playerID - localPlayerIndex  + 3) % 3;
        print($"playerId:{playerID} || localId:{localPlayerIndex}");
        var parent = isLocalPlayer ? localPlayerPlayerCardPlace : parentPlaces[localTransformIndex];
        GameObject cardPre = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        cardPre.name = card.Name;

        GameObject back = cardPre.transform.GetChild(0).gameObject;
        back.SetActive(true);
        
        cardPre.transform.SetParent(decksPos, false);
        cardPre.transform.SetParent(parent.transform, true);
       
        cardPre.GetComponent<Image>().sprite = cardsSprite.FirstOrDefault(s => s.name == card.Name);

        cardPre.LeanMoveLocal(pos, 0.5f).setEaseInQuad();

        if (!isLocalPlayer) return;
       // cardPre.transform.localPosition = pos;
        cardPre.transform.localRotation = Quaternion.Euler(0, 0, angleZ);
        cardPre.transform.localScale = Vector2.one * localScale;




        cardPre.LeanMoveLocal(pos, 0.4f).setEaseInQuad().setOnComplete(()=> {
            // back.SetActive(!isLocalPlayer);
            Flip( cardPre);
         
        });
    }
    

    public void GetSumOfCardValue(List<CardData> cards, out int sum, bool isLocalPlayer = true)
    {
        
        sum = cards.Sum(c => c.value);
        if (sum >= 10)
        {
            sum = sum % 10;
        }
        if (!isLocalPlayer) return;
        localSumOfCardValueText.text = sum.ToString();
    }

    private void ShowCard(List<GameObject> gameObjects,bool isRemove)
    {
        gameObjects.ForEach((g) =>
        {
            if (isRemove) Destroy(g);
            else
                if (g.transform.childCount > 0)
            {
                // g.transform.GetChild(0).gameObject.SetActive(false);

                Flip(g);
            }
                   
                       
        });
    }

    private List<GameObject> GetChildGameObjectToList(GameObject gameObject)
    {
        List<GameObject> list = new List<GameObject>();
        //for(int i = 0; i < gameObject.transform.childCount; i++)
        //{
        //    list.Add(gameObject.transform.GetChild(i).gameObject);
        //}
        foreach (Transform transform in gameObject.transform) {
            list.Add(transform.gameObject);
        }
        return list;
    }


    public void Flip( GameObject card)
    {
        
            LeanTween.rotateY(card, 90f, 0.25f).setEaseInOutQuad().setOnComplete(() =>
            {
                card.transform.GetChild(0).gameObject.SetActive(false); // show back (child of front)
                LeanTween.rotateY(card, 0f, 0.25f).setEaseInOutQuad();
            });
        
        
    }


    [ClientRpc]
    public void RpcGameOver(ulong winnerNetId)
    {
        ShowCard(GetChildGameObjectToList(otherPlayerPlayerCardPlace),false);
        print("client id :" + NetworkClient.localPlayer.netId);
        calculateButton.gameObject.SetActive(false);
        clearButton.gameObject.SetActive(true);


        string message = (winnerNetId == NetworkClient.localPlayer.netId) ? "You Win!" : "You Lose!";
        finalText.text = message;
        // Debug.Log("Game Over: ");
    }

    [ClientRpc]
    private void DrawnRpc()
    {
        finalText.text = "Drawn!";
    }
}



//[Server]
//private void DealInitialHands()
//{
//    // ... (deck creation and shuffling logic) ...

//    // Get all connected player manager instances
//    var allPlayers = NetworkServer.connections.Values
//        .Select(conn => conn.identity.GetComponent<PlayerManager>())
//        .ToList();

//    if (allPlayers.Count != 4)
//    {
//        Debug.LogError("Need exactly 4 players to deal hands!");
//        return;
//    }

//    // 1. Determine the starting player index in the 'allPlayers' list
//    int startIndex = 0;
//    if (dealStarterConnectionId != -1)
//    {
//        for (int i = 0; i < allPlayers.Count; i++)
//        {
//            // Note: Use connectionId if that's what dealStarterConnectionId stores
//            if (allPlayers[i].connectionId == dealStarterConnectionId)
//            {
//                startIndex = i;
//                break;
//            }
//        }
//    }
//    // If dealStarterConnectionId is -1, startIndex remains 0, the default first player.

//    // 2. Create the ordered list for dealing
//    List<PlayerManager> dealingOrder = new List<PlayerManager>();
//    for (int i = 0; i < allPlayers.Count; i++)
//    {
//        // Use modulo to wrap around the player list
//        int playerIndex = (startIndex + i) % allPlayers.Count;
//        dealingOrder.Add(allPlayers[playerIndex]);
//    }

//    // 3. Deal 2 cards to each player using the new order
//    for (int i = 0; i < 2; i++)
//    {
//        foreach (var player in dealingOrder)
//        {
//            if (deck.Count > 0)
//            {
//                Card dealtCard = deck[0];
//                deck.RemoveAt(0);
//                player.DealCard(dealtCard);
//            }
//        }
//    }
//    // ...
//}
