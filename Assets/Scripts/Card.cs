using Unity.Netcode;
using UnityEngine;

public struct Card : INetworkSerializable
{
    //escolhi usar struct porque as cartas serão imutáveis
    public int Value { get; private set; }
    public Enum_Suit Suit { get; private set; }


    //separar valor real da carta do valor do jogo
    public int BlackjackValue
    {
        get
        {
            if (Value > 10)
                return 10;

            return Value;
        }
    }

    public Card(int value, Enum_Suit suit)
    {
        Value = value;
        Suit = suit;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int value = Value;
        Enum_Suit suit = Suit;

        serializer.SerializeValue(ref value);
        serializer.SerializeValue(ref suit);

        if (serializer.IsReader)
        {
            Value = value;
            Suit = suit;
        }
    }
}
