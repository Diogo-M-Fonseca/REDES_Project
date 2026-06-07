using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Trata de todo o UI 
/// </summary>
public class GameUi : MonoBehaviour
{
    /// <summary>
    /// Instancia singleton para acesso global
    /// </summary>
    public static GameUi Instance;

    /// <summary>
    /// Prefab da carta visual
    /// </summary>
    [Header("Card prefab")]
    [SerializeField] private GameObject cardPrefab;
    /// <summary>
    /// Local onde as cartas do dealer são empilhadas
    /// </summary>
    [Header("Spots")]
    [SerializeField] private Transform dealerSpot;
    /// <summary>
    /// Local onde as cartas do jogador 1 são empilhadas
    /// </summary>
    [SerializeField] private Transform player1Spot;
    /// <summary>
    ///  Local onde as cartas do jogador 2 são empilhadas
    /// </summary>
    [SerializeField] private Transform player2Spot;


    /// <summary>
    /// Botão de "Pedir carta" (Hit)
    /// </summary>
    [Header("Buttons")]
    [SerializeField] private GameObject hitButton;
    /// <summary>
    /// Botão de "Pedir para Parar" (Stand)
    /// </summary>
    [SerializeField] private GameObject standButton;
    /// <summary>
    /// Botão de "Pedir para sair do jogo"
    /// </summary>
    [SerializeField] private GameObject closeGameOverButton;
    /// <summary>
    /// Botão de "Pedir para jogar de novo" (Replay)
    /// </summary>
    [SerializeField] private GameObject playAgainButton;

    /// <summary>
    /// Texto do valor da mão do jogador 1
    /// </summary>
    [Header("Texts")]
    [SerializeField] private TMP_Text player1Value;
    /// <summary>
    /// Texto do valor da mão do jogador 2
    /// </summary>
    [SerializeField] private TMP_Text player2Value;
    /// <summary>
    /// Texto do valor da mão do dealer
    /// </summary>
    [SerializeField] private TMP_Text dealerValue;
    /// <summary>
    /// Texto do Titulo do fim de jogo
    /// </summary>
    [SerializeField] private TMP_Text gameOverTitle;
    /// <summary>
    /// Texto da descrição do fim de jogo
    /// </summary>
    [SerializeField] private TMP_Text gameOverDescription;

    /// <summary>
    /// Tela de fim de jogo
    /// </summary>
    [Header("GameOver Panel")]
    [SerializeField] private GameObject gameOverPanel;

    /// <summary>
    /// Seta indicadora do turno do jogador 1
    /// </summary>
    [Header("Turn Arrows")]
    [SerializeField] private GameObject player1TurnArrow;
    /// <summary>
    /// Seta indicadora do turno do jogador 2
    /// </summary>
    [SerializeField] private GameObject player2TurnArrow;
    /// <summary>
    /// Seta indicadora do turno do dealer
    /// </summary>
    [SerializeField] private GameObject dealerTurnArrow;

    /// <summary>
    /// Dicionario que mapeia o ID do jogador para o Transform do Spot
    /// </summary>
    private readonly Dictionary<ulong, Transform> playerSpots = new();
    /// <summary>
    /// Dicionário que guarda quantas cartas o jogador já tem
    /// </summary>
    private readonly Dictionary<ulong, int> cardCount = new();

    /// <summary>
    /// Referencia ao singleton do GameManager
    /// </summary>
    private GameManager gm;

    /// <summary>
    /// Indica se o UI já configurou os dois jogadores
    /// </summary>
    private bool isReady = false;

    /// <summary>
    /// Queue de cartas que chegaram antes do UI estar pronto
    /// </summary>
    private readonly Queue<(ulong, Card)> pendingCards = new();

    private void Awake()
    {
        //Implementação do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //só tem uma scene no jogo todo então não implementei DontDestoyOnLoad
    }
    private void Start()
    {
        //obter a referencia do GameManager
        gm = GameManager.Instance;

        if (gm != null)
        {
            //Insrição nas mudanças de NetworkVariables do GameManager
            gm.CurrentTurn.OnValueChanged += OnTurnChanged;

            gm.CurrentPlayerIndex.OnValueChanged += OnCurrentPlayerIndexChanged;

            gm.Player1Id.OnValueChanged += OnPlayerIdsChanged;
            gm.Player2Id.OnValueChanged += OnPlayerIdsChanged;

            gm.Player1HandValue.OnValueChanged += OnPlayer1HandValueChanged;
            gm.Player2HandValue.OnValueChanged += OnPlayer2HandValueChanged;
            gm.DealerHandValue.OnValueChanged += OnDealerHandValueChanged;

            SetupPlayers();

            //chamo manualmente o callback do turno inicial para configurar UI
            OnTurnChanged(gm.CurrentTurn.Value, gm.CurrentTurn.Value);
        }

    }

    /// <summary>
    /// Executado quando o valor da mão do jogador 1 muda no servidor
    /// Altera o texto da pontuação do player 1
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void OnPlayer1HandValueChanged(int oldValue, int newValue)
    {
        if (player1Value != null)
            player1Value.text = $"Value: {newValue}";
    }

    /// <summary>
    /// Executado quando o valor da mão do jogador 2 muda no servidor
    /// Altera o texto da pontuação do player 2
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void OnPlayer2HandValueChanged(int oldValue, int newValue)
    {
        if (player2Value != null)
            player2Value.text = $"Value: {newValue}";
    }

    /// <summary>
    /// Executado quando o valor da mão do dealer muda no servidor
    /// Altera o texto da pontuação do dealer 
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void OnDealerHandValueChanged(int oldValue, int newValue)
    {
        if (dealerValue != null)
            dealerValue.text = $"Value: {newValue}";
    }

    /// <summary>
    /// Chamado quando o indice de qual jogador está a jogar muda
    /// Ativa os botões e indicadores adequados
    /// </summary>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    private void OnCurrentPlayerIndexChanged(int oldIndex, int newIndex)
    {

        UpdateTurnArrow();

        if (gm == null) return;
        //Verifica se o turno é do cliente local
        bool myTurn = gm.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);

        //Ativa/desativa botões de Hit e Stand adequadamente
        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);
    }

    /// <summary>
    /// Chamado quando o Ids do jogadores mudam (como quando conectam/reconectam)
    /// </summary>
    /// <param name="oldId"></param>
    /// <param name="newId"></param>
    private void OnPlayerIdsChanged(ulong oldId, ulong newId)
    {
        SetupPlayers();
    }

    /// <summary>
    /// Configura os spots visuais para cada jogador baseado nos IDs
    /// </summary>
    private void SetupPlayers()
    {
        if (gm == null) return;

        playerSpots.Clear();
        cardCount.Clear();

        // Verifica se o PlayerId é válido
        if (gm.Player1Id.Value != ulong.MaxValue)
        {
            playerSpots[gm.Player1Id.Value] = player1Spot;
            cardCount[gm.Player1Id.Value] = 0;
        }

        if (gm.Player2Id.Value != ulong.MaxValue)
        {
            playerSpots[gm.Player2Id.Value] = player2Spot;
            cardCount[gm.Player2Id.Value] = 0;
        }

        //Se ambos os jogadores já estão presentes o UI está pronto para processar cartas pendentes
        bool bothPlayersReady = (gm.Player1Id.Value != ulong.MaxValue && gm.Player2Id.Value != ulong.MaxValue);

        if (bothPlayersReady)
        {
            isReady = true;
            ProcessPendingCards();
        }
    }

    /// <summary>
    /// Chamado quando o turno do jogo muda
    /// </summary>
    /// <param name="oldTurn"></param>
    /// <param name="newTurn"></param>
    private void OnTurnChanged(Enum_Turn oldTurn, Enum_Turn newTurn)
    {
        UpdateTurnArrow();

        //Quando o turno passa para o dealer, fecha o painel de game over se estiver aberto
        if (newTurn == Enum_Turn.dealer && dealerTurnArrow != null)
        {
            GameOverPanelClose();
        }

        if (gm == null) gm = GameManager.Instance;
        if (gm == null) return;

        // Verifica se é a vez do cliente local
        // Esta secção do código é igual ao OnCurrentPlayerIndexChanged depropósito
        // Fazem o mesmo mas em contextos e situações diferentes 
        bool myTurn = gm.IsPlayerTurn(NetworkManager.Singleton.LocalClientId);

        hitButton.SetActive(myTurn);
        standButton.SetActive(myTurn);

        //Garantir que fecha o painel de gameover 
        if (newTurn == Enum_Turn.dealing || newTurn == Enum_Turn.player)
        {
            GameOverPanelClose();
        }

    }

    /// <summary>
    /// Atualiza a exibição das setas indicadoras
    /// </summary>
    private void UpdateTurnArrow()
    {
        if (gm == null) return;

        //Desativa todas as setas por precaução
        player1TurnArrow.SetActive(false);
        player2TurnArrow.SetActive(false);
        dealerTurnArrow.SetActive(false);

        Enum_Turn turn = gm.CurrentTurn.Value;

        //Ativa a seta indicada dependendo de quem é a vez
        if (turn == Enum_Turn.player)
        {
            int currentIndex = gm.CurrentPlayerIndex.Value;

            if (currentIndex == 0 && player1TurnArrow != null)
            {
                player1TurnArrow.SetActive(true);
            }
            else if (currentIndex == 1 && player2TurnArrow != null)
            {
                player2TurnArrow.SetActive(true);
            }
        }
        else if (turn == Enum_Turn.dealer && dealerTurnArrow != null)
        {
            dealerTurnArrow.SetActive(true);
        }
    }

    /// <summary>
    /// Adicona o visual da carta ao spot do jogador especifico
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="card"></param>
    public void AddCardToPlayer(ulong playerId, Card card)
    {
        // Se o UI ainda não estiver pronto, mete a carta na QUEUE
        if (!isReady)
        {
            pendingCards.Enqueue((playerId, card));
            return;
        }

        if (!playerSpots.ContainsKey(playerId))
        {
            return;
        }

        Transform spot = playerSpots[playerId];

        GameObject cardObj = Instantiate(cardPrefab);

        cardObj.transform.SetParent(spot);

        //Calcula um offset horizontal baseado em quantas cartas já existem para criar um efeito "leque" com as cartas
        float offset = cardCount[playerId] * 1f;

        cardObj.transform.localPosition = Vector3.right * offset ;

        cardObj.GetComponent<CardView>().SetSprite(card);

        //Incrementa contagem das cartas deste jogador
        cardCount[playerId]++;

    }

    /// <summary>
    /// Processa a QUEUE de cartas pendentes
    /// </summary>
    private void ProcessPendingCards()
    {
        while (pendingCards.Count > 0)
        {
            var (playerId, card) = pendingCards.Dequeue();
            AddCardToPlayer (playerId, card);
        }
    }

    /// <summary>
    /// Adiciona uma carta visual ao dealer
    /// </summary>
    /// <param name="card"></param>
    public void AddCardToDealer(Card card)
    {
        GameObject cardObj = Instantiate(cardPrefab);
        cardObj.transform.SetParent(dealerSpot);

        int dealerCards = dealerSpot.childCount;

        //Mesma lógica do offset das cartas do jogador
        float offset = dealerCards * 1f;

        cardObj.transform.localPosition = Vector3.right * offset;
        cardObj.GetComponent<CardView>().SetSprite(card);
    }

    /// <summary>
    /// Mostra o painel gameover com o titulo e descrição
    /// </summary>
    /// <param name="title"></param>
    /// <param name="resultText"></param>
    public void ShowResult(string title, string resultText)
    {
        gameOverPanel.SetActive(true);
        gameOverTitle.text = title;
        gameOverDescription.text = resultText;
        playAgainButton.SetActive(true);
        closeGameOverButton.SetActive(true);
    }

    /// <summary>
    /// Feixa o painel de Gameover
    /// </summary>
    public void GameOverPanelClose()
    {
        gameOverPanel.SetActive(false);
        playAgainButton.SetActive(false);
        closeGameOverButton.SetActive(false);
    }

    /// <summary>
    /// Pede ao servidor para jogar outra vez
    /// </summary>
    public void PlayAgain()
    {
        if(gm != null && Player.LocalPlayer != null)
        {
            //Envia Rpc para o servidor para socilitar nova rodada
            gm.PlayAgainServerRpc(NetworkManager.Singleton.LocalClientId);
            //Desativar botão para evitar multiplos cliques
            playAgainButton.SetActive(false);
        }
    }

    /// <summary>
    /// Pede ao servidor para Disconectar e fechar a aplicação
    /// </summary>
    public void Exit()
    {
        // Se for servidor desliga e sai
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
        }
        else
        {
            // Se for cliente envia rpc para o servidor para avisar que vai sair
            if (Player.LocalPlayer != null)
            {
                Player.LocalPlayer.ExitGameServerRpc();
            }

            //Aguardar um pouco para a mensagem ser enviada e então sai localmente
            Invoke(nameof(QuitGameLocally), 0.2f);
        }
    }

    /// <summary>
    /// Fecha o jogo localmente (cliente)
    /// </summary>
    public void QuitGameLocally()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        Application.Quit();
    }

    /// <summary>
    /// Limpa toda as cartas
    /// </summary>
    /// <param name="spot"></param>
    public void ClearSpot(Transform spot)
    {
        foreach (Transform child in spot)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Limpa toda a mesa reiniciando os contadores de cartas
    /// </summary>
    public void ClearTable()
    {
        ClearSpot(dealerSpot);
        ClearSpot(player1Spot);
        ClearSpot(player2Spot);

        cardCount.Clear();

        //Reinsere no dicionario os jogadores conhecidos
        foreach (ulong clientId in playerSpots.Keys)
        {
            cardCount.Add(clientId, 0);
        }
    }

    /// <summary>
    /// Metodo chamado pelo botão Hit
    /// </summary>
    public void Hit()
    {
        if(Player.LocalPlayer == null) return;
        //Vai buscar o objeto do player e pede ao servidor para dar hit
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().HitServerRpc();
    }

    /// <summary>
    /// Metodo chamado pelo botão Stand
    /// </summary>
    public void Stand()
    {
        if (Player.LocalPlayer == null) return;
        //Vai buscar o objeto do player e pede ao servidor para dar stand
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().StandServerRpc();
    }
}
