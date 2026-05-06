using UnityEngine;

public struct Card 
{
    //escolhi usar struct porque as cartas serão imutáveis
    public int Value {  get; set; }
    public Enum_Suit Suit { get; }

    public Card(int value, Enum_Suit suit)
    {
        Value = value;
        Suit = suit;
    }
}
