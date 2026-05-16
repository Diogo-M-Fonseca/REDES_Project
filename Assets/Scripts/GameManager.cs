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

    private float offSet = 0.8f;

    void Start()
    {
        StartGame();
    }

    private void SpawnCard(Card card, Transform parent)
    {
        CardView view = Instantiate(cardPrefab, parent);

        int index = parent.childCount -1;

        view.transform.localPosition = new Vector3(index * offSet, 0, 0);

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
            Debug.Log("jogador ganhou");
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

    private void TurnDealer()
    {
        //Turno do Dealer
        while (dealerHand.GetHandValue() < 17)
        {
            GiveDealer();
        }

        //verifica se alguém venceu
        Conclusion();
    }

    public void HitButton()
    {
        GivePlayer();

        if (playerHand.IsBust())
        {
            Debug.Log("Derrota");
        }
    }

    public void StandButton()
    {
        TurnDealer(); 
    }

    private void FirstRound()
    {
        GivePlayer();
        GiveDealer();
        GivePlayer();
        GiveDealer();
    }

    public void StartGame()
    {
        ClearTable();

        deck = new Deck();
        deck.Initialize();

        playerHand = new Hand();
        dealerHand = new Hand();

        FirstRound();
    }

    private void ClearTable()
    {
        foreach (Transform child in playerArea)
        {
            Destroy(child);
        }

        foreach (Transform child in dealerArea)
        {
            Destroy(child);
        }
    }



}
