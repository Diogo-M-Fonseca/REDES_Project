using UnityEngine;

public class GameManager : MonoBehaviour
{


    // ATENÇÃO ESTE CÓDIGO TERÁ QUE SER FEITO DO INICIO DEVIDO AO ONLINE
    //é favor não criar apego emocional :)


    private Deck deck;
    private Hand playerHand;
    private Hand dealerHand;

    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform playerArea;
    [SerializeField] private Transform dealerArea;

    void Start()
    {
        deck = new Deck();
        deck.Initialize();

        playerHand = new Hand();
        dealerHand = new Hand();


    }

    private void SpawnCard(Card card, Transform parent)
    {
        CardView view = Instantiate(cardPrefab, parent);

        view.SetSprite(card);

    }

    private void GivePlayer()
    {
        Card card = deck.Draw();
        playerHand.AddCard(card);

        SpawnCard(card, playerArea);
    }

    private void GiveDealer()
    {
        Card card = deck.Draw();
        dealerHand.AddCard(card);

        SpawnCard(card, dealerArea);
    }

    private void Conclusion()
    {
        
        //lembrar de tentar fazer switch dnv mais tarde

        int playerValue = playerHand.GetHandValue();
        int dealerValue = dealerHand.GetHandValue();


        //tentei fazer switch e não consegui :(
        if (playerValue > 21)
        {
            Debug.Log("Dealer ganhou");
        }
        else if (dealerValue > 21)
        {
            Debug.Log("Jogador ganhou");
        }
        else if (playerValue > dealerValue)
        {
            Debug.Log("Dealer ganhou");
        }
        else if (playerValue < dealerValue)
        {
            Debug.Log("Dealer ganhou");
        }
        else
        {
            Debug.Log("Empate");
        }

    }


}
