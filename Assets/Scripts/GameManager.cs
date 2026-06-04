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

    private int currentPlayerIndex = 0;

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

        deck = new Deck();
        deck.Initialize();

        currentTurn = Enum_Turn.waiting;
        currentPlayerIndex = 0;
    }

    public void Registration(ulong clientId)
    {
        if (!IsServer) return;
        
        if (players.Exists(p => p.clientId == clientId)) return;

        players.Add(new PlayerData(clientId));

        if (players.Count >= 2)
        {
            StartRound();
        }
    }

    public PlayerData GetCurrentPlayer()
    {
        if (players.Count == 0) return null;
        return players[currentPlayerIndex];
    }

    public void NextTurn()
    {
        if (!IsServer) return;
        
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

        PlayerData dealer = new PlayerData(999); // Fake Id

        dealer.Hit(deck.Draw());
        dealer.Hit(deck.Draw());

        while (dealer.HandValue < 17)
        {
            dealer.Hit(deck.Draw());   
        }
        Conclusion(dealer);
    }


    public void Conclusion(PlayerData dealer)
    {
        currentTurn = Enum_Turn.Finished;


        foreach (PlayerData player in players)
        {
            if (player.IsBust())
            {
                continue;
            }
            else if (dealer.IsBust() || player.hand.GetHandValue() > dealer.hand.GetHandValue())
            {
                // Player wins
            }
            else if (player.hand.GetHandValue() < dealer.hand.GetHandValue())
            {
                // Player loses
            }
            else
            {
                // Push
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
    }


    public void DealFirstCards()
    {
        foreach (PlayerData player in players)
        {
            player.Hit(deck.Draw());
            player.Hit(deck.Draw());
        }
    }

    public void StartRound()
    {
        currentTurn = Enum_Turn.dealing;

        deck = new Deck();
        deck.Initialize();

        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        DealFirstCards();

        currentPlayerIndex = 0;
        currentTurn = Enum_Turn.player;
    }

    public void PlayerHit(ulong clientId)
    {
        if (!IsServer) return;
        if(currentTurn != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        player.Hit(deck.Draw());

        if (player.IsBust())
        {
            player.Stand();
            NextTurn();
        }

    }


    private PlayerData GetPlayer(ulong clientId)
    {
        return players.Find(p => p.clientId == clientId);
    }

    public void PlayerStand(ulong clientId)
    {
        if (!IsServer) return;
        PlayerData player = GetPlayer(clientId);
        if (player == null) return;
        player.Stand();
        NextTurn();
    }





}
