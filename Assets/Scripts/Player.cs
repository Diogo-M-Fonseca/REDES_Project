using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalPlayer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
        }
    }



    [ServerRpc]
    public void HitServerRpc()
    {

        GameManager.Instance.PlayerHit(OwnerClientId);
    }

    [ServerRpc]
    public void StandServerRpc()
    {

        GameManager.Instance.PlayerStand(OwnerClientId);
    }
}