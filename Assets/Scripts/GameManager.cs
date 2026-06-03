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

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (!IsServer) return;

        deck = new Deck();
        deck.Initialize();

        currentTurn = Enum_Turn.waiting;
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
        if (players.Count == 0) return;

        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
    }

    public void DealerTurn()
    {
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

        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        deck = new Deck();
        deck.Initialize();

        DealFirstCards();

        currentPlayerIndex = 0;
        currentTurn = Enum_Turn.player;
    }

}
