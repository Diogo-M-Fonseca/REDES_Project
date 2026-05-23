using UnityEngine;

public class PlayerData
{
    public ulong clientId { get; private set; }
    public Hand hand { get; private set; }
    public bool isStanding { get; private set; }

    public PlayerData(ulong ClientId)
    {
        clientId = ClientId;
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





}
