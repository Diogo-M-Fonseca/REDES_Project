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
        DontDestroyOnLoad(gameObject);

    }
    private void Start()
    {
        gm = GameManager.Instance;

        if (gm != null)
        {
            gm.CurrentTurn.OnValueChanged += OnTurnChanged;

            // Forçar setup após 1 segundo (dar tempo para IDs serem configurados)
            Invoke(nameof(DelayedSetup), 0.5f);
        }

    }

    //so pra teste 
    private void DelayedSetup()
    {
        SetupPlayers();
    }

    private void SetupPlayers()
    {
        if (gm == null) gm = GameManager.Instance;
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
    }

    private void OnTurnChanged(Enum_Turn oldTurn, Enum_Turn newTurn)
    {
        turn.text = $"Turn: {newTurn}";
        
        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        bool myTurn = gm.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);

        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);
    }

    public void AddCardToPlayer(ulong playerId, Card card)
    {
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
        if(Player.LocalPlayer == null) return;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().HitServerRpc();
    }

    public void Stand()
    {
        if (Player.LocalPlayer == null) return;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().StandServerRpc();
    }
}
