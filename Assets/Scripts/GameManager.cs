using UnityEngine;
using TMPro;
using Unity.VisualScripting;

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
    [SerializeField] private TMP_Text playerPontuation;
    [SerializeField] private TMP_Text dealerPontuation;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text pontuationText;

    private float offSet = 0.8f;
    private bool endGame;

    void Start()
    {
        StartGame();
    }

    private void SpawnCard(Card card, Transform parent)
    {
        CardView view = Instantiate(cardPrefab, parent);

        int index;

        if (parent == playerArea)
        {
            index = playerHand.CardCount - 1;
        }
        else
        {
            index = dealerHand.CardCount;
        }

        view.transform.localPosition = new Vector3(index * offSet, 0f, 0f);

        view.SetSprite(card);

    }

    private void GivePlayer()
    {
        Card card = deck.Draw();
        playerHand.AddCard(card);

        SpawnCard(card, playerArea);

        ChangePontuation();
    }

    private void GiveDealer()
    {
        Card card = deck.Draw();
        dealerHand.AddCard(card);

        SpawnCard(card, dealerArea);

        ChangePontuation();
    }

    private void Conclusion()
    {
        
        int playerValue = playerHand.GetHandValue();
        int dealerValue = dealerHand.GetHandValue();

        bool playerBJ = playerHand.HasBlackJack();
        bool dealerBJ = dealerHand.HasBlackJack();

        if (playerBJ && !dealerBJ)
        {
            ShowGameOver("Player ganhou com Blackjack!", true);
            return;
        }
        else if (dealerBJ && !playerBJ)
        {
            ShowGameOver("Dealer ganhou com Blackjack!", false);
            return;
        }
        else if(playerValue > 21)
        {
            ShowGameOver("Dealer ganhou (Player Bust)", false);
            return;
        }
        else if (dealerValue > 21)
        {
            ShowGameOver("Player ganhou (Dealer Bust)", playerBJ);
            return;
        }
        else if (playerValue > dealerValue)
        {
            ShowGameOver("Player ganhou ", playerBJ);
            return;
        }
        else if (playerValue < dealerValue)
        {
            ShowGameOver("Dealer ganhou", playerBJ);
            return;
        }
        else
        {
            ShowGameOver("Empate", playerBJ);
            return;
        }

    }

    private void TurnDealer()
    {
        //Turno do Dealer
        while (dealerHand.GetHandValue() < 17)
        {
            GiveDealer();

            if (dealerHand.IsBust())
            {
                Conclusion();
                return;
            }
        }

        ChangePontuation();

        //verifica se alguém venceu
        Conclusion();
    }

    public void HitButton()
    {
        if (endGame) return;

        GivePlayer();

        if (playerHand.IsBust())
        {
            Conclusion();
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

        if (playerHand.HasBlackJack() || dealerHand.HasBlackJack())
        {
            Conclusion();
        }
    }

    public void StartGame()
    {
        ClearTable();
        endGame = false;

        deck = new Deck();
        deck.Initialize();

        playerHand = new Hand();
        dealerHand = new Hand();

        FirstRound();
        ChangePontuation();
    }

    private void ClearTable()
    {
        foreach (Transform child in playerArea)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in dealerArea)
        {
            Destroy(child.gameObject);
        }

    }

    private void ChangePontuation()
    {
        playerPontuation.text = playerHand.GetHandValue().ToString();
        dealerPontuation.text = dealerHand.GetHandValue().ToString();
    }

    private void ShowGameOver(string result, bool playerBlackjack)
    {
        endGame = true;
        gameOverScreen.SetActive(true);

        int playerValue = playerHand.GetHandValue();
        int dealerValue = dealerHand.GetHandValue();

        string blackjackText = playerBlackjack ? "(Blackjack!)" : "";

        resultText.text = result + blackjackText;

        pontuationText.text = $"Player: {playerValue} \n Dealer: {dealerValue}";
    }

    public void RestartButton()
    {
        gameOverScreen.SetActive(false);

        StartGame();
    }


}
