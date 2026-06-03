using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [ServerRpc]
    public void HitRPC()
    {
        GameManager.Instance.PlayerHit(OwnerClientId);
    }

    [ServerRpc]
    public void StandRPC()
    {
        GameManager.Instance.PlayerStand(OwnerClientId);
    }
}