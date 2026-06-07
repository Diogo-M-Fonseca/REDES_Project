using Unity.Netcode;
using UnityEngine;

/// <summary>
/// struct que representa uma carta do baralho serializavel pela rede (INetworkSerializable)
/// </summary>
//escolhi usar struct porque as cartas serão imutáveis
public struct Card : INetworkSerializable
{
    /// <summary>
    /// valor das cartas
    /// </summary>
    //com private set para garantir que não é alterado posteriormente
    public int Value { get; private set; }
    /// <summary>
    /// Naipe da Carta
    /// </summary>
    // também imutável posteriormente
    public Enum_Suit Suit { get; private set; }

    /// <summary>
    /// separar valor real da carta do valor do jogo
    /// Em blackjack cartas como o rei que valem 13 pontos são reduzidos para o valor maximo
    /// </summary>
    //O valor maximo de blackjack é 10
    //O valor de Ás só é alterado na Hand 
    public int BlackjackValue
    {
        get
        {
            if (Value > 10)
                return 10;

            return Value;
        }
    }

    /// <summary>
    /// Contrutor que cria uma nova carta com valor e naipe
    /// </summary>
    /// <param name="value"></param>
    /// <param name="suit"></param>
    public Card(int value, Enum_Suit suit)
    {
        Value = value;
        Suit = suit;
    }

    /// <summary>
    /// Define como serializar/deserializar através da rede
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializer"></param>
    //Metodo da interface INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        //Cria variaveis locais para os valores atuais de struct
        int value = Value;
        Enum_Suit suit = Suit;

        //Serializa value no buffer
        serializer.SerializeValue(ref value);
        //Serializa suit no buffer
        serializer.SerializeValue(ref suit);

        //Se for leitor atualiza valores
        //Só vai ser true se tiver a discerializar
        if (serializer.IsReader)
        {
            Value = value;
            Suit = suit;
        }
    }
}
