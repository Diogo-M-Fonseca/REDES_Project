using UnityEngine;

public struct Card 
{
    //escolhi usar struct porque as cartas serão imutáveis
    public int Value {  get; set; }

    public Card(int value)
    {
        Value = value;
    }
}
