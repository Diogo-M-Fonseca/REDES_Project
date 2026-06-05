using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUi : MonoBehaviour
{
    public static GameUi Instance;

    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform dealerSpot;
    [SerializeField] private Transform player1Spot;
    [SerializeField] private Transform player2Spot;

    [SerializeField] private GameObject hitButton;
    [SerializeField] private GameObject standButton;

    [SerializeField] private TMP_Text turn;
    [SerializeField] private TMP_Text result;

    private readonly Dictionary<ulong, Transform> playerSpots = new();
    private readonly Dictionary<ulong, int> cardCount = new();

    private GameManager gm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    private void Start()
    {
        gm = GameManager.Instance;

        if (gm != null)
        {
            gm.CurrentTurn.OnValueChanged += OnTurnChanged;
            SetupPlayers();
        }

    }

    private void SetupPlayers()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds;

        int index = 0;

        foreach (ulong clientId in clients)
        {
            if (index == 0)
            {
                playerSpots.Add(clientId, player1Spot);
            }
            else if (index == 1)
            {
                playerSpots.Add(clientId, player2Spot);
            }

            cardCount.Add(clientId, 0);

            index++;
        }

    }

    private void OnTurnChanged(Enum_Turn oldTurn, Enum_Turn newTurn)
    {
        turn.text = $"Turn: {newTurn}";
        
        bool myTurn = GameManager.Instance.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);

        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);
    }

    public void AddCardToPlayer(ulong playerId, Card card)
    {
        if (!playerSpots.ContainsKey(playerId))
        {
            return;
        }

        Transform spot = playerSpots[playerId];

        GameObject cardObj = Instantiate(cardPrefab);

        float offset = cardCount[playerId] * 0.5f;

        cardObj.transform.position = spot.position + Vector3.right * offset;

        cardObj.GetComponent<CardView>().SetSprite(card);

        cardCount[playerId]++;

    }

    public void AddCardToDealer(Card card)
    {
        GameObject cardObj = Instantiate(cardPrefab);

        int dealerCards = dealerSpot.childCount;

        float offset = dealerCards * 0.5f;

        cardObj.transform.position = dealerSpot.position + Vector3.right * offset;
        cardObj.GetComponent<CardView>().SetSprite(card);
    }

    public void ShowResult(string resultText)
    {
        result.text = resultText;
    }

    public void ClearSpot(Transform spot)
    {
        foreach (Transform child in spot)
        {
            Destroy(child.gameObject);
        }
    }
    public void ClearTable()
    {
        ClearSpot(dealerSpot);
        ClearSpot(player1Spot);
        ClearSpot(player2Spot);

        cardCount.Clear();

        foreach (ulong clientId in playerSpots.Keys)
        {
            cardCount.Add(clientId, 0);
        }

        ShowResult(string.Empty);
    }

    public void Hit()
    {
        if(Player.LocalPlayer != null) return;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().HitServerRpc();
    }
}
