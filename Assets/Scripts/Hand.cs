using System.Collections.Generic;
using UnityEngine;

public class Hand 
{
    private readonly List<Card> cards = new();

    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    public int GetHandValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            total += card.BlackjackValue;

            if (card.Value == 1)
            {
                aceCount++;
            }
        }

        while (aceCount > 0 && total + 10 <= 21)
        {
            total += 10;
            aceCount--;
        }

        return total;
    }
   
}
