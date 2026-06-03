using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private Deck deck;

    private NetworkVariable<Enum_Turn> currentTurn = new(Enum_Turn.waiting);

    int currentPlayerIndex = 0;

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

}
