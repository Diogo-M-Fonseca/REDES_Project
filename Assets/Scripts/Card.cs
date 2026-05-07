using UnityEngine;

public struct Card 
{
    //escolhi usar struct porque as cartas serão imutáveis
    public int Value {  get;  }
    public Enum_Suit Suit { get; }


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
}
