using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private Deck deck;

    private Enum_Turn currentTurn;

    private readonly List<PlayerData> players = new();
    private readonly Hand dealerHand = new();

    private int currentPlayerIndex;

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

        StartRound();
    }

    public void Registration(ulong clientId)
    {
        if (!IsServer) return;
        
        if (GetPlayer(clientId) != null) return;

        players.Add(new PlayerData(clientId));

        if (players.Count >= 2)
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
        currentTurn = Enum_Turn.dealer;
        SyncTurnClientRpc(currentTurn);

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
        currentTurn = Enum_Turn.Finished;
        SyncTurnClientRpc(currentTurn);

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
        currentPlayerIndex = 0;

        foreach (PlayerData player in players)
        {
            player.Clear();
        }
        dealerHand.Clear();
    }


    public void DealFirstCards()
    {
        foreach (PlayerData player in players)
        {
            GiveCardToPlayer(player);
            GiveCardToPlayer(player);
        }

        dealerHand.AddCard(deck.Draw());
        dealerHand.AddCard(deck.Draw());

        
    }

    public void StartRound()
    {
        deck = new Deck();
        deck.Initialize();
        dealerHand.Clear();
        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        currentPlayerIndex = 0;
        currentTurn = Enum_Turn.dealing;

        SyncTurnClientRpc(currentTurn);

        DealFirstCards();

        currentTurn = Enum_Turn.player;
        SyncTurnClientRpc(currentTurn);

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
        if(currentTurn != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

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

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        player.Stand();
        NextTurn();
    }

    [ClientRpc]
    private void SyncTurnClientRpc(Enum_Turn turn)
    {
        Debug.Log($"Turn changed to: {turn}");
    }

    [ClientRpc]
    private void SendCardClientRpc(ulong clientId, Card card)
    {
        Debug.Log($"Card sent to client {clientId}: {card.Value} of {card.Suit}");
    }

    [ClientRpc]
    private void DealerCardDrawnClientRpc(Card card)
    {
        Debug.Log($"Dealer drew: {card.Value} of {card.Suit}");
    }


    [ClientRpc]
    private void OnPlayerWinClientRpc(ulong clientId)
    {
        Debug.Log($"Player {clientId} wins!");
    }
    
    [ClientRpc]
    private void OnPlayerLoseClientRpc(ulong clientId)
    {
        Debug.Log($"Player {clientId} loses!");
    }

    [ClientRpc]
    private void OnPlayerPushClientRpc(ulong clientId)
    {
        Debug.Log($"Player {clientId} pushes!");
    }
}
