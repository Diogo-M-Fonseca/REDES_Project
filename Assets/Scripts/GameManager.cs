using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private Deck deck;

    private NetworkVariable<Enum_Turn> currentTurn = new(Enum_Turn.waiting);

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

    }

    public void Registration(ulong clientId)
    {
        if (!IsServer) return;
        
        if (players.Exists(p => p.clientId == clientId)) return;

        players.Add(new PlayerData(clientId));
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

    public Card DrawCard()
    {
        return deck.Draw();
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

}
