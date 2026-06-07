using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa as cartas que cada entidade tem
/// </summary>
//Não herda monobehaviour porque não estará em menhum gameobject
public class Hand 
{
    /// <summary>
    /// Lista que armazena as artas atuais na mão
    /// </summary>
    //readonly para evitar que seja alterada
    private readonly List<Card> cards = new();

    /// <summary>
    /// Retorna a quantidade de cartas na mão
    /// </summary>
    //manter a imutabilidade faznedo propriedade só de leitura
    public int CardCount => cards.Count;

    /// <summary>
    /// Adiciona uma carta á mão
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(Card card)
    {
        cards.Add(card);
    }

    /// <summary>
    /// Calcula o valor total da mão
    /// </summary>
    /// <returns></returns>
    public int GetHandValue()
    {
        int total = 0; // Soma dos valores

        //Em blackjack o ás tem um valor mutável dependendo de quantos tens em mãos
        int aceCount = 0; //Contador de azes

        //Loop passa por todas as cartas no array
        foreach (Card card in cards)
        {
            //Adiciona ao total o valor de blackjack da carta
            total += card.BlackjackValue;

            //Apenas o ás tem como valor 1
            if (card.Value == 1)
            {
                aceCount++; //Aumenta o contador de azes
            }
        }

        //Caso o valor de 11 do Ás de bust na entidade mudar pra 1 caso contrário usar o 11
        while (aceCount > 0 && total + 10 <= 21)
        {
            total += 10;
            aceCount--;
        }

        return total;
    }

    /// <summary>
    /// Verifica se a mão deu Bust
    /// </summary>
    /// <returns></returns>
    public bool IsBust()
    {
        return GetHandValue() > 21;
    }
   
    /// <summary>
    /// Verifica se a mão é um blackjack natural (2 cartas)
    /// </summary>
    /// <returns></returns>
    // Em blackjack um blackjack de 2 cartas vale mais que um de 3
    public bool HasBlackJack()
    {
        return cards.Count == 2 && GetHandValue() == 21;
    }

    /// <summary>
    /// Limpa todas as cartas da mão
    /// </summary>
    public void Clear()
    {
        cards.Clear();
    }
}
