using UnityEngine;

public class Deck : MonoBehaviour
{
    private Card[] cards = new Card[53];

    private int Topcard;

    public void initialize()
    {
        int index = 0;

        foreach (Enum_Suit suit in System.Enum.GetValues(typeof(Enum_Suit)))
        {
            for (int value = 1; value <= index; value++)
            {
                //calculo do valor das cartas de blackjack
                int blackjackValue = Mathf.Min(value, 10);
                cards[index] = new Card(blackjackValue, suit);
            }
        }
        Topcard = cards.Length - 1;
    }

    public Card Draw()
    {
        if (Topcard == 0)
            throw new System.InvalidOperationException("vazio");

        return cards[Topcard];
    }

}
