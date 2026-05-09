using UnityEngine;

public class Deck 
{
    private Card[] cards = new Card[52];

    private int topCard;

    public void Initialize()
    {
        int index = 0;

        foreach (Enum_Suit suit in System.Enum.GetValues(typeof(Enum_Suit)))
        {
            for (int value = 1; value <= 13; value++)
            {
                //calculo do valor das cartas de blackjack
                int blackjackValue = Mathf.Min(value, 10);
                cards[index] = new Card(value, suit);
                index++;
            }
        }
        Shuffle();

        topCard = cards.Length - 1;
    }

    public Card Draw()
    {
        if (topCard < 0)
            throw new System.InvalidOperationException("vazio");

        Card card = cards[topCard];
        topCard--;

        return card;
    }


    private void Shuffle()
    {
        for (int i = cards.Length - 1; i > 0; i--)
        {
            //algoritmo fisher-yates??
            int randomIndex = Random.Range(0, i + 1);

            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

}
