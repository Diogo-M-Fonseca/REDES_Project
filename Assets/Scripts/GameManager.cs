using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private Deck deck;

    private void Start()
    {
        if (IsServer)
        {
            deck = new Deck();
            deck.Initialize();
        }
    }

}
