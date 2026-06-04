using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private readonly ulong clientId;
    private readonly Hand hand = new();
    private bool isStanding;

    public int HandValue => hand.GetHandValue();
    public ulong ClientId => clientId;
    public Hand Hand => hand;
    public bool IsStanding => isStanding;


    public PlayerData(ulong clientId)
    {
        this.clientId = clientId;
    }


    public void Hit(Card card)
    {
        hand.AddCard(card);
    }

    public void Stand()
    {
        isStanding = true;
    }

    public void Clear()
    {
        hand.Clear();
        isStanding = false;
    }

    public bool IsBust()
    {
        return hand.IsBust();
    }

    public bool HasBlackJack()
    {
        return hand.HasBlackJack();
    }

}
