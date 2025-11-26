
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Collections;
using Mirror.BouncyCastle.Security.Certificates;



public class CardGameController : NetworkBehaviour
{

    public static CardGameController instance;
    public PlayerNet playerOne;
    public PlayerNet playerTwo;
    public SyncDictionary<string, PlayerNet> playersDic = new SyncDictionary<string, PlayerNet>();

    [Serializable]
    public class Player
    {
        public List<Card> hands;
    }

    [SerializeField] public List<Sprite> cardsSprite = new List<Sprite>();
    [SerializeField] private List<Card> decks = new List<Card>();

    public List<Player> players = new List<Player>();
    public Player firstPlayer;
    public Player secondPlayer;

    [Header("CARD PREFAB")]
    [SerializeField] private GameObject cardPrefab;

    [Header("LOCATION OF CARD")]
    [SerializeField] private Transform decksPos;
    [SerializeField] private GameObject localPlayerPlayerCardPlace;
    [SerializeField] private GameObject otherPlayerPlayerCardPlace;
    [Header("UI")]
    [Header("TEXT ELEMENT")]
    [SerializeField] private Text localSumOfCardValueText;
    [SerializeField] private Text otherSumOfCardValueText;
    [SerializeField] private Text finalText;
    [Header("BUTTON ELEMENT")]
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button calculateButton;
    [SerializeField] private Button clearButton;

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
    public void OnServerConnect()
    {

    }
    public void OnServerDisconnect()
    {

    }
    public void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
       
        int playerCount = NetworkManager.singleton.numPlayers;
        Debug.Log($"{nameof(CardGameController)}: OnServerAddPlayer| PlayerCount:{playerCount}");
        if(playerCount == 2 )
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
    }
    #endregion





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        calculateButton.gameObject.SetActive(false);
        clearButton.gameObject.SetActive(false);
        shuffleButton.gameObject.SetActive(false);

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

    [ClientRpc]
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
        yield return new WaitForSeconds(1f);
        DealCard();
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

        print($"{playerOne.netId}:{playerOne.sum} | {playerTwo.netId}:{playerTwo.sum}");
     
        if (playerOne.sum == playerTwo.sum)
        {

            DrawnRpc();
            return;

        }
        RpcGameOver(playerOne.sum > playerTwo.sum ? playerOne.netId  : playerTwo.netId);


        
    }
   

    int maxCard = 4;
    int maxCardPerPlayer = 2;
    private IEnumerator ShuffleCard()
    {
        for (int i = 0; i < maxCard; i++) {
            var card = decks[i];
            if (i % 2 == 0)
            {


                //firstPlayer.hands.Add(card);
                playerOne.hands.Add(new CardData
                {
                    Name = card.Name,
                    value = card.value
                });
           

              
              
              

            }
            else
            {
               playerTwo.hands.Add(new CardData
              {
                  Name = card.Name,
                  value = card.value
              });
 
            }

            decks.Remove(card);
            yield return new WaitForSeconds(0.5f);
        }
       // localSumOfCardValueText.text = GetSumOfCardValue(firstPlayer.hands).ToString();
    }

    [ServerCallback]
    public void DrawnCard(NetworkConnectionToClient conn)
    {
        var card = decks[0];
        var data = new CardData() { Name = card.Name, value = card.value };

        if (conn == playerOne.connectionToClient) playerOne.hands.Add(data); else playerTwo.hands.Add(data);
      
        
    }

   

   
   
    private void GenerateCardFromCardSprite()
    {
        playerOne.hands.Clear();
        playerTwo.hands.Clear();
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



    public void CloneCardGameObject(CardData card, bool isLocalPlayer, Vector2 pos, float localScale, float angleZ)
    {
        var parent = isLocalPlayer ? localPlayerPlayerCardPlace : otherPlayerPlayerCardPlace;
        GameObject cardPre = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);

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
