using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class GameUi : MonoBehaviour
{
    public static GameUi Instance;

    [Header("Card prefab")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Spots")]
    [SerializeField] private Transform dealerSpot;
    [SerializeField] private Transform player1Spot;
    [SerializeField] private Transform player2Spot;

    [Header("Buttons")]
    [SerializeField] private GameObject hitButton;
    [SerializeField] private GameObject standButton;
    [SerializeField] private GameObject closeGameOverButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text player1Value;
    [SerializeField] private TMP_Text player2Value;
    [SerializeField] private TMP_Text dealerValue;
    [SerializeField] private TMP_Text gameOverTitle;
    [SerializeField] private TMP_Text gameOverDescription;

    [Header("GameOver Panel")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Turn Arrows")]
    [SerializeField] private GameObject player1TurnArrow;
    [SerializeField] private GameObject player2TurnArrow;
    [SerializeField] private GameObject dealerTurnArrow;

    private readonly Dictionary<ulong, Transform> playerSpots = new();
    private readonly Dictionary<ulong, int> cardCount = new();

    private GameManager gm;

    private bool isReady = false;
    private readonly Queue<(ulong, Card)> pendingCards = new();

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

            gm.CurrentPlayerIndex.OnValueChanged += OnCurrentPlayerIndexChanged;

            gm.Player1Id.OnValueChanged += OnPlayerIdsChanged;
            gm.Player2Id.OnValueChanged += OnPlayerIdsChanged;

            gm.Player1HandValue.OnValueChanged += OnPlayer1HandValueChanged;
            gm.Player2HandValue.OnValueChanged += OnPlayer2HandValueChanged;
            gm.DealerHandValue.OnValueChanged += OnDealerHandValueChanged;

            SetupPlayers();

            OnTurnChanged(gm.CurrentTurn.Value, gm.CurrentTurn.Value);
        }

    }

    private void OnPlayer1HandValueChanged(int oldValue, int newValue)
    {
        if (player1Value != null)
            player1Value.text = $"Value: {newValue}";
    }

    private void OnPlayer2HandValueChanged(int oldValue, int newValue)
    {
        if (player2Value != null)
            player2Value.text = $"Value: {newValue}";
    }

    private void OnDealerHandValueChanged(int oldValue, int newValue)
    {
        if (dealerValue != null)
            dealerValue.text = $"Value: {newValue}";
    }

    private void OnCurrentPlayerIndexChanged(int oldIndex, int newIndex)
    {

        UpdateTurnArrow();

        if (gm == null) return;
        bool myTurn = gm.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);
        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);
    }


    private void OnPlayerIdsChanged(ulong oldId, ulong newId)
    {
        SetupPlayers();
    }

    private void SetupPlayers()
    {
        if (gm == null) return;

        playerSpots.Clear();
        cardCount.Clear();

        if (gm.Player1Id.Value != ulong.MaxValue)
        {
            playerSpots[gm.Player1Id.Value] = player1Spot;
            cardCount[gm.Player1Id.Value] = 0;
            Debug.Log($"[UI] Player 1 ({gm.Player1Id.Value}) assigned to spot 1");
        }

        if (gm.Player2Id.Value != ulong.MaxValue)
        {
            playerSpots[gm.Player2Id.Value] = player2Spot;
            cardCount[gm.Player2Id.Value] = 0;
            Debug.Log($"[UI] Player 2 ({gm.Player2Id.Value}) assigned to spot 2");
        }

        bool bothPlayersReady = (gm.Player1Id.Value != ulong.MaxValue && gm.Player2Id.Value != ulong.MaxValue);

        if (bothPlayersReady)
        {
            isReady = true;
            Debug.Log("[UI] Ready processing pending cards");
            ProcessPendingCards();
        }
    }

    private void OnTurnChanged(Enum_Turn oldTurn, Enum_Turn newTurn)
    {
        UpdateTurnArrow();
        
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        bool myTurn = gm.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);

        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);

        if (newTurn == Enum_Turn.dealing || newTurn == Enum_Turn.player)
        {
            GameOverPanelClose();
        }
    }

    private void UpdateTurnArrow()
    {
        if (gm == null) return;

        player1TurnArrow.SetActive(false);
        player2TurnArrow.SetActive(false);
        dealerTurnArrow.SetActive(false);

        Enum_Turn turn = gm.CurrentTurn.Value;

        if (turn == Enum_Turn.player)
        {
            int currentIndex = gm.CurrentPlayerIndex.Value;

            if (currentIndex == 0 && player1TurnArrow != null)
            {
                player1TurnArrow.SetActive(true);
            }
            else if (currentIndex == 1 && player2TurnArrow != null)
            {
                player2TurnArrow.SetActive(true);
            }
        }
        else if (turn == Enum_Turn.dealer && dealerTurnArrow != null)
        {
            dealerTurnArrow.SetActive(true);
        }
    }

    public void AddCardToPlayer(ulong playerId, Card card)
    {
        if (!isReady)
        {
            pendingCards.Enqueue((playerId, card));
            Debug.Log($"[UI] Not ready yet");
            return;
        }

        if (!playerSpots.ContainsKey(playerId))
        {
            Debug.LogWarning($"[UI] No spot found for player {playerId}");
            return;
        }

        Transform spot = playerSpots[playerId];

        GameObject cardObj = Instantiate(cardPrefab);

        float offset = cardCount[playerId] * 0.5f;

        cardObj.transform.position = spot.position + Vector3.right * offset;

        cardObj.GetComponent<CardView>().SetSprite(card);

        cardCount[playerId]++;

    }

    private void ProcessPendingCards()
    {
        while (pendingCards.Count > 0)
        {
            var (playerId, card) = pendingCards.Dequeue();
            AddCardToPlayer (playerId, card);
        }
    }


    public void AddCardToDealer(Card card)
    {
        GameObject cardObj = Instantiate(cardPrefab);

        int dealerCards = dealerSpot.childCount;

        float offset = dealerCards * 0.5f;

        cardObj.transform.position = dealerSpot.position + Vector3.right * offset;
        cardObj.GetComponent<CardView>().SetSprite(card);
    }

    public void ShowResult(string title, string resultText)
    {
        gameOverPanel.SetActive(true);
        gameOverTitle.text = title;
        gameOverDescription.text = resultText;
    }

    public void GameOverPanelClose()
    {
        gameOverPanel.SetActive(false);
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
    }

    public void Hit()
    {
        if(Player.LocalPlayer == null) return;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().HitServerRpc();
    }

    public void Stand()
    {
        if (Player.LocalPlayer == null) return;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().StandServerRpc();
    }
}
