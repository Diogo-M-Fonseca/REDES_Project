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

    public NetworkVariable<int> CurrentPlayerIndex = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private bool roundActive;

    public NetworkVariable<ulong> Player1Id = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<ulong> Player2Id = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> Player1HandValue = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> Player2HandValue = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> DealerHandValue = new(
        0,
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
        CurrentPlayerIndex.Value++;

        if (CurrentPlayerIndex.Value >= players.Count)
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
            UpdateDealerHandValue();
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

            string dealerReason;
            string resultType;

            if (!playerBust && (dealerBust||player.HandValue > dealerHand.GetHandValue()))
            {
                dealerReason = dealerBust ? "Dealer busted! You win." : "You have a higher score.";
                resultType = "win";
            }
            else if (playerBust || player.HandValue < dealerHand.GetHandValue())
            {
                dealerReason = playerBust ? "You busted! You lose." : "Dealer have a higher score.";
                resultType= "lose";
            }
            else
            {
                dealerReason = "DRAW";
                resultType = "push";
            }
            string pvpMessage = GetPvpMessage(player);
            string completeMessage = dealerReason + "\n\n" + pvpMessage;

            if(resultType == "win") 
                OnPlayerWinClientRpc(player.ClientId, completeMessage);
            if(resultType == "lose")
                OnPlayerLoseClientRpc(player.ClientId, completeMessage);
            else
                OnPlayerPushClientRpc(player.ClientId, completeMessage);
        }

        EndRound();
    }


    private string GetPvpMessage(PlayerData Currentplayer)
    {
        if (players.Count < 2) return "Waiting for other player.";

       PlayerData other = players[0] == Currentplayer ? players[1] : players[0];
       bool currentBust = Currentplayer.IsBust();
       bool otherBust = other.IsBust();

        if (currentBust && otherBust) return "Both players busted - pvp draw.";
        if (currentBust) return "You busted - opponent wins pvp.";
        if (otherBust) return "Opponent busted - you win pvp";

        int currentVal = Currentplayer.HandValue;
        int otherVal = other.HandValue;

        if (currentVal > otherVal) return "You beat the opponent";
        if (currentVal < otherVal) return "Opponent beats you";

        return "pvp draw - same score";
    }


    public void EndRound()
    {
        roundActive = false;
        CurrentPlayerIndex.Value = 0;

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
        UpdateDealerHandValue();
        DealerCardDrawnClientRpc(dealerCard1);

        Card dealerCard2 = deck.Draw();
        dealerHand.AddCard(dealerCard2);
        UpdateDealerHandValue();
        DealerCardDrawnClientRpc(dealerCard2);
    }

    public void StartRound()
    {
        if (!IsServer || roundActive) return;
        roundActive = true;

        Player1HandValue.Value = 0;
        Player2HandValue.Value = 0;
        DealerHandValue.Value = 0;

        deck = new Deck();
        deck.Initialize();
        dealerHand.Clear();

        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        CurrentPlayerIndex.Value = 0;
        CurrentTurn.Value = Enum_Turn.dealing;

        DealFirstCards();

        CurrentTurn.Value = Enum_Turn.player;

    }

    private void GiveCardToPlayer(PlayerData player)
    {
        Card card = deck.Draw();
        player.Hit(card);
        UpdatePlayerHandValue(player.ClientId);
        SendCardClientRpc(player.ClientId, card);
    }

    public void PlayerHit(ulong clientId)
    {
        if (!IsServer) return;
        if(CurrentTurn.Value != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        if (players[CurrentPlayerIndex.Value].ClientId != clientId) return;

        Card card = deck.Draw();
        player.Hit(card);

        UpdatePlayerHandValue(player.ClientId);

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

        if(players[CurrentPlayerIndex.Value].ClientId != clientId) return;

        player.Stand();
        NextTurn();
    }

    private void UpdatePlayerHandValue(ulong clientId)
    {
        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        if (clientId == Player1Id.Value)
        {
            Player1HandValue.Value = player.HandValue;
        }
        else if (clientId == Player2Id.Value)
        {
            Player2HandValue.Value = player.HandValue;
        }
    }

    private void UpdateDealerHandValue()
    {
        DealerHandValue.Value = dealerHand.GetHandValue();
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
        if (CurrentTurn.Value != Enum_Turn.player) return false; 

        if (!IsServer)
        {
            int index = CurrentPlayerIndex.Value;

            if (index == 0 && Player1Id.Value == clientId) return true;
            if (index == 1 && Player2Id.Value == clientId) return true;

            return false;
        }



        if (players.Count == 0) return false;
        if (CurrentPlayerIndex.Value >= players.Count) return false;

        return players[CurrentPlayerIndex.Value].ClientId == clientId;
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
    private void OnPlayerWinClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU WIN", reason);
    }
    
    [ClientRpc]
    private void OnPlayerLoseClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU LOST", reason);
    }

    [ClientRpc]
    private void OnPlayerPushClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU PUSH", reason);
    }
}
