using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void HitServerRpc()
    {

        GameManager.Instance.PlayerHit(OwnerClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    public void StandServerRpc()
    {

        GameManager.Instance.PlayerStand(OwnerClientId);
    }
}