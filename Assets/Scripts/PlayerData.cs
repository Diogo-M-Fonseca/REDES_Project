using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Armazena dados de um jogador durante uma partida de Blackjack
/// </summary>
//Não herda nada porque é suposto ser só um armazém de dados
public class PlayerData 
{
    /// <summary>
    /// Identificador unico do cliente (netcode)
    /// </summary>
    //readonly porque não muda pusteriormente
    private readonly ulong clientId;

    /// <summary>
    /// mão de cartas do jogador
    /// </summary>
    private readonly Hand hand = new();

    /// <summary>
    /// bool que indica se o jogador deu Stand ou não
    /// </summary>
    private bool isStanding;

    /// <summary>
    /// Propriedade para obter o valor da mão
    /// </summary>
    public int HandValue => hand.GetHandValue();

    /// <summary>
    /// Propriedade para acessar o clientId
    /// </summary>
    public ulong ClientId => clientId;

    /// <summary>
    /// Propriedade para acessar a mão
    /// </summary>
    public Hand Hand => hand;

    /// <summary>
    /// Propriedade para acessar isStanding
    /// </summary>
    public bool IsStanding => isStanding;


    /// <summary>
    /// Construtor que cria uma instancia de Playerdata usando o clientId recebido
    /// </summary>
    /// <param name="clientId"></param>
    public PlayerData(ulong clientId)
    {
        this.clientId = clientId;
    }


    /// <summary>
    /// Adiciona uma carta á mão do jogador (Hit)
    /// </summary>
    /// <param name="card"></param>
    public void Hit(Card card)
    {
        hand.AddCard(card);
    }

    /// <summary>
    /// Marca que o jogador deu Stand
    /// </summary>
    public void Stand()
    {
        isStanding = true;
    }

    /// <summary>
    /// Limpa a mão e o isStanding do jogador
    /// </summary>
    public void Clear()
    {
        hand.Clear();
        isStanding = false;
    }

    /// <summary>
    /// Verifica se o jogador deu bust
    /// </summary>
    /// <returns></returns>
    public bool IsBust()
    {
        return hand.IsBust();
    }

    /// <summary>
    /// Verifica se o jogador tem um blackjack natural
    /// </summary>
    /// <returns></returns>
    public bool HasBlackJack()
    {
        return hand.HasBlackJack();
    }

}
