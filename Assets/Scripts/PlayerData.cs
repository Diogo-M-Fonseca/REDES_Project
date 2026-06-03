using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public ulong clientId { get; private set; }
    public Hand hand { get; private set; }
    public bool isStanding { get; private set; }

    
    public PlayerData(ulong clientId)
    {
        this.clientId = clientId;
        hand = new Hand();
        isStanding = false;
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
