using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private Deck deck;

    public NetworkVariable<Enum_Turn> CurrentTurn = new(
        Enum_Turn.waiting,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly List<PlayerData> players = new();
    private readonly Hand dealerHand = new();

    private int currentPlayerIndex;
    private bool roundActive;

    public NetworkVariable<ulong> Player1Id = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<ulong> Player2Id = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void Registration(ulong clientId)
    {
        if (!IsServer) return;
        
        if (GetPlayer(clientId) != null) return;

        players.Add(new PlayerData(clientId));

        if (players.Count == 1)
        {
            Player1Id.Value = clientId;
        }
        else if (players.Count == 2)
        {
            Player2Id.Value = clientId;
        }

        if (players.Count == 2 && !roundActive)
        {
            StartRound();
        }
    }

    public void NextTurn()
    {  
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
        {
            DealerTurn();
            return;
        }
    }

    public void DealerTurn()
    {
        CurrentTurn.Value = Enum_Turn.dealer;

        while (dealerHand.GetHandValue() < 17)
        {
            Card card = deck.Draw();
            dealerHand.AddCard(card);
            DealerCardDrawnClientRpc(card);
        }

        Conclusion();
    }


    public void Conclusion()
    {
        CurrentTurn.Value = Enum_Turn.Finished;

        foreach (PlayerData player in players)
        {
            bool playerBust = player.IsBust();
            bool dealerBust = dealerHand.IsBust();

            if (!playerBust && (dealerBust||player.HandValue > dealerHand.GetHandValue()))
            {
                OnPlayerWinClientRpc(player.ClientId);
            }
            else if (playerBust || player.HandValue < dealerHand.GetHandValue())
            {
                OnPlayerLoseClientRpc(player.ClientId);
            }
            else
            {
                OnPlayerPushClientRpc(player.ClientId);
            }
        }

        EndRound();
    }



    public void EndRound()
    {
        roundActive = false;
        currentPlayerIndex = 0;

        foreach (PlayerData player in players)
        {
            player.Clear();
        }
        dealerHand.Clear();

        ClearTableClientRpc();

        CurrentTurn.Value = Enum_Turn.waiting;
    }


    public void DealFirstCards()
    {
        foreach (PlayerData player in players)
        {
            GiveCardToPlayer(player);
            GiveCardToPlayer(player);
        }

        Card dealerCard1 = deck.Draw();
        dealerHand.AddCard(dealerCard1);
        DealerCardDrawnClientRpc(dealerCard1);

        Card dealerCard2 = deck.Draw();
        dealerHand.AddCard(dealerCard2);
        DealerCardDrawnClientRpc(dealerCard2);
    }

    public void StartRound()
    {
        if (!IsServer || roundActive) return;
        roundActive = true;

        deck = new Deck();
        deck.Initialize();
        dealerHand.Clear();

        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        currentPlayerIndex = 0;
        CurrentTurn.Value = Enum_Turn.dealing;

        DealFirstCards();

        CurrentTurn.Value = Enum_Turn.player;

    }

    private void GiveCardToPlayer(PlayerData player)
    {
        Card card = deck.Draw();
        player.Hit(card);
        SendCardClientRpc(player.ClientId, card);
    }

    public void PlayerHit(ulong clientId)
    {
        if (!IsServer) return;
        if(CurrentTurn.Value != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        if (players[currentPlayerIndex].ClientId != clientId) return;

        Card card = deck.Draw();
        player.Hit(card);

        SendCardClientRpc(player.ClientId, card);

        if (player.IsBust())
        {
            player.Stand();
            NextTurn();
        }

    }


    private PlayerData GetPlayer(ulong clientId)
    {
        return players.Find(p => p.ClientId == clientId);
    }

    public void PlayerStand(ulong clientId)
    {
        if (!IsServer) return;
        if(CurrentTurn.Value != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        if(players[currentPlayerIndex].ClientId != clientId) return;

        player.Stand();
        NextTurn();
    }

    private void OnClientConnected(ulong clientId)
    {
        Registration(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        PlayerData player = GetPlayer(clientId);
        if (player != null)
        {
            players.Remove(player);
        }

        if (roundActive)
        {
            EndRound();
        }
    }

    public bool IsPlayerTurn(ulong clientId)
    {
        if (players.Count == 0) return false;

        if (currentPlayerIndex >= players.Count) return false;

        return CurrentTurn.Value == Enum_Turn.player && players[currentPlayerIndex].ClientId == clientId;
    }


    [ClientRpc]
    private void SendCardClientRpc(ulong clientId, Card card)
    {
        Debug.Log($"Player {clientId} drew: {card.Value} of {card.Suit}");
        Debug.Log($"[CLIENT RPC] Adding card for player {clientId} to UI");

        GameUi.Instance.AddCardToPlayer(clientId, card);
    }

    [ClientRpc]
    private void DealerCardDrawnClientRpc(Card card)
    {
        Debug.Log($"Dealer drew: {card.Value} of {card.Suit}");

        GameUi.Instance.AddCardToDealer(card);
    }

    [ClientRpc]
    private void ClearTableClientRpc()
    {
        GameUi.Instance.ClearTable();
    }

    [ClientRpc]
    private void OnPlayerWinClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU WIN");
    }
    
    [ClientRpc]
    private void OnPlayerLoseClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU LOSE");
    }

    [ClientRpc]
    private void OnPlayerPushClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU PUSH");
    }
}
