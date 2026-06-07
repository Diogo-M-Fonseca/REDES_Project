using UnityEngine;
/// <summary>
/// Classe responsável por gerenciar o baralho de 52 cartas
/// </summary>
//Não herda monobehaviour porque ao invés de estar num gameobject será usado pelo servidor para fazer calculos
//Quando o resutado desses calculos/metodos for concluido passará pro cliente em forma de Card
public class Deck 
{
    /// <summary>
    /// Array fixo de 52 cartas
    /// </summary>
    private Card[] cards = new Card[52];

    /// <summary>
    /// Indice da carta no topo do baralho
    /// </summary>
    private int topCard;

    /// <summary>
    /// Inicializa o baralho
    /// </summary>
    public void Initialize()
    {
        //Indice atual que vou usar para percurrer o array
        int index = 0;

        //Percorre todos os valores de Suit
        //System.Enum.GetValues(typeof(Enum_Suit) facilita o trabalho porque assim não tenho que fazer um array á parte
        foreach (Enum_Suit suit in System.Enum.GetValues(typeof(Enum_Suit)))
        {
            //Para cada naipe 13 vezes (porque cada naipe tem 13 cartas)
            for (int value = 1; value <= 13; value++)
            {
                //Cria uma nova carta com o valor e naipe adequado
                //adiciona ao array
                cards[index] = new Card(value, suit);

                //acança para o proximo indice do array de cartas
                index++;
            }
        }
        //baralhar o array depois de o preencher
        Shuffle();

        //Define o topo do baralho
        topCard = cards.Length - 1;
    }

    /// <summary>
    /// Saca cartas pelo topo do baralho e dá return da mesma
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    public Card Draw()
    {
        //Verifica se ainda há cartas no baralho
        if (topCard < 0)
            //Lança uma exceção para indicar o erro
            throw new System.InvalidOperationException("vazio");

        //obtem a carta no indice atual
        Card card = cards[topCard];

        //desce o indice (efitivamente a carta ainda lá está mas é como se não estivesse)
        topCard--;

        //retorna carta sacada
        return card;
    }

    /// <summary>
    /// Baralha as cartas usando Fisher-Yates
    /// </summary>
    private void Shuffle()
    {
        // Loop de trás pra frente, do ultimo indice até o segundo
        // O primeiro indice não faria sentido ser trocado consigo mesmo
        for (int i = cards.Length - 1; i > 0; i--)
        {
            //Gera um Indice aleatório entre 0 e i
            //algoritmo fisher-yates??
            int randomIndex = Random.Range(0, i + 1);

            //Troca as posições i e randomIndex
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

}
