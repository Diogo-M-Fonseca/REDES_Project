using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Representa o jogador conectado ao jogo
/// </summary>
//Herda NetworkBehaviour, permitindo comunicação via RPCs e propriedades de rede
//Cada cliente obrigatóriamente necessita possuir um Player.cs 
public class Player : NetworkBehaviour
{
    /// <summary>
    /// Referencia estática para o objecto Player do cliente local (jogador)
    /// </summary>
    //Util para UI e Rpcs sem precisar procurar o gameobject
    public static Player LocalPlayer;

    /// <summary>
    /// Verifica se o NetworkObject é propriedade do cliente local
    /// </summary>
    //Chamado quando o NetworkObject é spawnado na rede
    public override void OnNetworkSpawn()
    {
        //IsOwner é true apenas no cliente que possui este objecto
        if (IsOwner)
        {
            LocalPlayer = this;
        }
    }

    /// <summary>
    /// Solicita uma carta (Hit) ao servidor
    /// </summary>
    //Apenas o dono deste objecto pode chamar este rpc o que impede de cartas serem atribuidas ao jogador errado
    [ServerRpc]
    public void HitServerRpc()
    {
        //Servidor valida se pode dar hit
        GameManager.Instance.PlayerHit(OwnerClientId);
    }

    /// <summary>
    /// Solicita que o servidor pare (Stand)
    /// </summary>
    //Apenas o dono deste objecto pode chamar este rpc o que impede o jogador errado ser afetado
    [ServerRpc]
    public void StandServerRpc()
    {
        //Servidor valida se pode dar Stand
        GameManager.Instance.PlayerStand(OwnerClientId);
    }


    /// <summary>
    /// Solicita a saida do jogo (disconexão voluntária)
    /// </summary>
    [ServerRpc]
    public void ExitGameServerRpc()
    {
        //Servidor valida se pode disconectar
        GameManager.Instance.ExitGameServerRpc(OwnerClientId);
    }
}