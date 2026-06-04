using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [ServerRpc]
    public void HitServerRpc()
    {
        if (!IsOwner) return;

        GameManager.Instance.PlayerHit(OwnerClientId);
    }

    [ServerRpc]
    public void StandServerRpc()
    {
        if (!IsOwner) return;

        GameManager.Instance.PlayerStand(OwnerClientId);
    }
}