using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private Deck deck;

    private NetworkVariable<Enum_Turn> currentTurn = new(Enum_Turn.waiting);

    int currentPlayerIndex = 0;

    private void Start()
    {
        if (IsServer)
        {
            deck = new Deck();
            deck.Initialize();
        }
    }

}
