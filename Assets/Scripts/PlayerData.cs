using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public ulong clientId { get; private set; }
    public Hand hand { get; private set; }
    public bool isStanding { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        hand = new Hand();
        isStanding = false;
    }


    [ServerRpc]
    public void HitRpc()
    {
        if (!IsServer) return;
        if (isStanding) return;

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
