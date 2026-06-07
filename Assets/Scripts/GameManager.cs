using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Classe principal que cuida de toda a logica do jogo
/// </summary>
public class GameManager : NetworkBehaviour
{
    /// <summary>
    /// Instancia singleton para acesso global
    /// </summary>
    public static GameManager Instance;

    /// <summary>
    /// Baralho (server athoriative) criado a cada novo jogo
    /// </summary>
    private Deck deck;

    /// <summary>
    /// NetworkVariable do turno atual do jogo
    /// </summary>
    public NetworkVariable<Enum_Turn> CurrentTurn = new(
        Enum_Turn.waiting, //Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// Lista de dados do jogador
    /// </summary>
    private readonly List<PlayerData> players = new();

    /// <summary>
    /// Mão do dealer
    /// </summary>
    private readonly Hand dealerHand = new();


    /// <summary>
    /// NetworkVariable do indice do jogador atual na lista "players"
    /// </summary>
    public NetworkVariable<int> CurrentPlayerIndex = new(
        0, // Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// Bool que indica se a ronda está em andamento
    /// </summary>
    private bool roundActive;

    /// <summary>
    /// NetworkVariable do Id do jogador 1 
    /// Uso MaxValue como "vazio"
    /// </summary>
    public NetworkVariable<ulong> Player1Id = new(
        ulong.MaxValue, // Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// NetworkVariable do Id do jogador 2 
    /// Uso MaxValue como "vazio"
    /// </summary>
    public NetworkVariable<ulong> Player2Id = new(
        ulong.MaxValue, // Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// NetworkVariable do valor da mão do jogador 1 
    /// </summary>
    public NetworkVariable<int> Player1HandValue = new(
        0, // Valor inicial 
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// NetworkVariable do valor da mão do jogador 2 
    /// </summary>
    public NetworkVariable<int> Player2HandValue = new(
        0, // Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)

    /// <summary>
    /// NetworkVariable do valor da mão do Dealer 
    /// </summary>
    public NetworkVariable<int> DealerHandValue = new(
        0, // Valor inicial
        NetworkVariableReadPermission.Everyone, // Quem pode ler (todos)
        NetworkVariableWritePermission.Server); // Quem pode alterar (server)


    /// <summary>
    /// Conjunto que guarda quais clientes pediram "jogar novamente"
    /// </summary>
    private HashSet<ulong> readyToPlayAgain = new HashSet<ulong>();



    private void Awake()
    {
        //Implemtentação simples de Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //como só tem uma scene não usei DontDestroyOnLoad
    }

    /// <summary>
    /// Chamado quando o NetworkObject é spawnado
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Apenas o servidor precisa se inscrever nos eventos de conexão/disconexão
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    /// <summary>
    /// Chamado quando o NetworkObject é despawnado
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    /// <summary>
    /// Registra um novo Jogador
    /// </summary>
    /// <param name="clientId"></param>
    public void Registration(ulong clientId)
    {
        if (!IsServer) return;
        
        // Se o jogador já estiver registrado ignora
        if (GetPlayer(clientId) != null) return;

        // Cria um novo objecto PlayerData e adiciona à lista
        players.Add(new PlayerData(clientId));

        // Atribui o clientId ao primeiro ou segundo slot
        // Como o jogo tem um maximo de 2 jogadores se um terceiro jogador tentar entrar será ignorado
        if (players.Count == 1)
        {
            Player1Id.Value = clientId;
        }
        else if (players.Count == 2)
        {
            Player2Id.Value = clientId;
        }

        // Se já yemos 2 jogadores iniciar o jogo
        if (players.Count == 2 && !roundActive)
        {
            StartRound();
        }
    }

    /// <summary>
    /// Avança para o próximo jogador durante o turno dos jogadores
    /// </summary>
    public void NextTurn()
    {  
        //Incrementa o indice
        CurrentPlayerIndex.Value++;

        // Se já passou do ultimo jogador inicia o turno do dealer
        if (CurrentPlayerIndex.Value >= players.Count)
        {
            DealerTurn();
            return;
        }
    }
    /// <summary>
    /// Executa o turno do Dealer (server)
    /// </summary>
    public void DealerTurn()
    {
        CurrentTurn.Value = Enum_Turn.dealer;

        // Dealer saca cartas até ter 17 ou mais
        while (dealerHand.GetHandValue() < 17)
        {
            Card card = deck.Draw();
            dealerHand.AddCard(card);
            UpdateDealerHandValue();
            //Envia viual da carta para todos os clientes
            DealerCardDrawnClientRpc(card);
        }

        // Termina a rodada
        Conclusion();
    }

    /// <summary>
    /// Determina o resultado de cada jogador contra o dealer e contra o outro jogador
    /// </summary>
    public void Conclusion()
    {
        CurrentTurn.Value = Enum_Turn.Finished;

        // Vai a todos os jogadores registrados
        foreach (PlayerData player in players)
        {
            bool playerBust = player.IsBust();
            bool dealerBust = dealerHand.IsBust();
            bool playerBlackjack = player.HasBlackJack();
            bool dealerBlackjack = dealerHand.HasBlackJack();

            // Mensagem da comparação com o dealer
            string dealerReason;
            // Win, lose ou push
            string resultType;

            //Regras de comparação com o dealer
            if (playerBlackjack && !dealerBlackjack)
            {
                resultType = "win";
                dealerReason = "Blackjack! you win!";
            }
            else if (dealerBlackjack && !playerBlackjack)
            {
                resultType = "lose";
                dealerReason = "Dealer has a Blackjack. You lose.";
            }
            else if (playerBust)
            {
                resultType = "lose";
                dealerReason = "You busted! You lose.";
            }
            else if (dealerBust)
            {
                resultType = "win";
                dealerReason = "Dealer busted! You win.";
            }
            else if (player.HandValue > dealerHand.GetHandValue())
            {
                resultType = "win";
                dealerReason = "You have a higher score.";
            }
            else if (player.HandValue < dealerHand.GetHandValue())
            {
                resultType = "lose";
                dealerReason = "Dealer has a higher score.";
            }
            else
            {
                resultType = "push";
                dealerReason = "Same score. Push.";
            }

            // Mensagem comparando com o outro jogador
            string pvpMessage = GetPvpMessage(player);
            // Mensagem completa 
            string completeMessage = dealerReason + "\n\n" + pvpMessage;

            //Chamo o ClientRpc especifico para cada jogador e mostro o resultado
            switch (resultType)
            {
                case "win": OnPlayerWinClientRpc(player.ClientId, completeMessage); break;
                case "lose": OnPlayerLoseClientRpc(player.ClientId, completeMessage); break;
                default: OnPlayerPushClientRpc(player.ClientId, completeMessage); break;
            }
        }

        EndRound();
    }

    /// <summary>
    /// Gera uma mensagem de comparação entre os dois jogadores
    /// </summary>
    /// <param name="Currentplayer"></param>
    /// <returns></returns>
    private string GetPvpMessage(PlayerData Currentplayer)
    {
        // Se só existir um jogador não há pvp
        if (players.Count < 2) return "Waiting for other player.";

        // Procura o "outro" jogador
        PlayerData other = players[0] == Currentplayer ? players[1] : players[0];
        bool currentBust = Currentplayer.IsBust();
        bool otherBust = other.IsBust();

        if (currentBust && otherBust) return "Both players busted - pvp draw.";
        if (currentBust) return "You busted - opponent wins pvp.";
        if (otherBust) return "Opponent busted - you win pvp";

        int currentVal = Currentplayer.HandValue;
        int otherVal = other.HandValue;

        if (currentVal > otherVal) return "You beat the opponent";
        if (currentVal < otherVal) return "Opponent beats you";

        return "pvp draw - same score";
    }

    /// <summary>
    /// Encerra a rodada atual
    /// </summary>
    public void EndRound()
    {
        roundActive = false;
        CurrentPlayerIndex.Value = 0;

        // Limpa todos os jogadores (mãos e bools)
        foreach (PlayerData player in players)
        {
            player.Clear();
        }
        dealerHand.Clear();

        ClearTableClientRpc();

        // Volta ao estado de espera 
        CurrentTurn.Value = Enum_Turn.waiting;
    }

    /// <summary>
    /// Distribui as duas cartas iniciais para cada jogador e duas para o dealer
    /// </summary>
    public void DealFirstCards()
    {
        // Duas cartas para cada jogador
        foreach (PlayerData player in players)
        {
            GiveCardToPlayer(player);
            GiveCardToPlayer(player);
        }

        // Primeira carta do dealer
        Card dealerCard1 = deck.Draw();
        dealerHand.AddCard(dealerCard1);
        UpdateDealerHandValue();
        DealerCardDrawnClientRpc(dealerCard1);

        // Segunda carta do dealer
        Card dealerCard2 = deck.Draw();
        dealerHand.AddCard(dealerCard2);
        UpdateDealerHandValue();
        DealerCardDrawnClientRpc(dealerCard2);
    }

    /// <summary>
    /// Inicia uma nova rodada
    /// </summary>
    public void StartRound()
    {
        //reseta a coleção de jogadores prontos a jogar
        readyToPlayAgain.Clear();

        //Verificação de segurança
        if (!IsServer || roundActive) return;
        roundActive = true;

        ClearTableClientRpc();

        // Reseta os NetworkVariables de valores das mãos
        Player1HandValue.Value = 0;
        Player2HandValue.Value = 0;
        DealerHandValue.Value = 0;

        // Cria e embaralha um novo deck
        deck = new Deck();
        deck.Initialize();
        // Limpa a mão do dealer
        dealerHand.Clear();

        // Limpa todos os jogadores
        // (provavelmente a este ponto já tão limpos mas é melhor prevenir do que remediar)
        foreach (PlayerData player in players)
        {
            player.Clear();
        }

        // Reseta o indice do jogador atual
        CurrentPlayerIndex.Value = 0;

        CurrentTurn.Value = Enum_Turn.dealing;

        DealFirstCards();

        CurrentTurn.Value = Enum_Turn.player;

    }

    /// <summary>
    /// Dá a carta ao jogador e sicroniza
    /// </summary>
    /// <param name="player"></param>
    private void GiveCardToPlayer(PlayerData player)
    {
        Card card = deck.Draw();
        // Adiciona carta lógica
        player.Hit(card);
        UpdatePlayerHandValue(player.ClientId);
        // Envia a carta para o cliente
        SendCardClientRpc(player.ClientId, card);
    }

    /// <summary>
    /// Processa o pedido de Hit
    /// </summary>
    /// <param name="clientId"></param>
    public void PlayerHit(ulong clientId)
    {
        if (!IsServer) return;
        // Só pode fazer hit se for turno de player
        if(CurrentTurn.Value != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        // Verifica se é a vez deste jogador especifico
        if (players[CurrentPlayerIndex.Value].ClientId != clientId) return;

        Card card = deck.Draw();
        player.Hit(card);

        UpdatePlayerHandValue(player.ClientId);

        SendCardClientRpc(player.ClientId, card);

        // Se o jogador der Bust automaticamente acaba o turno
        if (player.IsBust())
        {
            player.Stand();
            NextTurn();
        }

    }

    /// <summary>
    /// Retorna o PlayerData correspondente ao clientId
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    private PlayerData GetPlayer(ulong clientId)
    {
        return players.Find(p => p.ClientId == clientId);
    }

    /// <summary>
    /// Processa o pedido de Stand
    /// </summary>
    /// <param name="clientId"></param>
    public void PlayerStand(ulong clientId)
    {
        if (!IsServer) return;
        // Só pode fazer hit se for turno de player
        if (CurrentTurn.Value != Enum_Turn.player) return;

        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        // Verifica se é a vez deste jogador especifico
        if (players[CurrentPlayerIndex.Value].ClientId != clientId) return;

        player.Stand();
        NextTurn();
    }

    /// <summary>
    /// Muda o valor da NetworkVariable de PlayerxHandValue
    /// </summary>
    /// <param name="clientId"></param>
    private void UpdatePlayerHandValue(ulong clientId)
    {
        PlayerData player = GetPlayer(clientId);
        if (player == null) return;

        // Identifica o jogador e atualiza o NetworkVariable correspondente
        if (clientId == Player1Id.Value)
        {
            Player1HandValue.Value = player.HandValue;
        }
        else if (clientId == Player2Id.Value)
        {
            Player2HandValue.Value = player.HandValue;
        }
    }

    /// <summary>
    /// Muda o valor da NetworkVariable de DealerHandValue
    /// </summary>
    private void UpdateDealerHandValue()
    {
        DealerHandValue.Value = dealerHand.GetHandValue();
    }

    /// <summary>
    /// Lida com a conexão do cliente
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientConnected(ulong clientId)
    {
        Registration(clientId);
    }

    /// <summary>
    /// Lida com a disconexão do cliente
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        // Remove o jogador da lista
        PlayerData player = GetPlayer(clientId);
        if (player != null)
        {
            players.Remove(player);
        }

        // Se a rounda tiver ativa encerra-a
        if (roundActive)
        {
            EndRound();
        }

        // Notifica o outro jogador
        NotifyOpponentExitClientRpc(clientId);
    }

    /// <summary>
    /// Verifica se é o turno do jogador indicado
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public bool IsPlayerTurn(ulong clientId)
    {
        if (CurrentTurn.Value != Enum_Turn.player) return false; 

        if (!IsServer)
        {
            // Uso diretamente os IDs e o Indice atual
            int index = CurrentPlayerIndex.Value;

            if (index == 0 && Player1Id.Value == clientId) return true;
            if (index == 1 && Player2Id.Value == clientId) return true;

            return false;
        }



        if (players.Count == 0) return false;
        if (CurrentPlayerIndex.Value >= players.Count) return false;

        return players[CurrentPlayerIndex.Value].ClientId == clientId;
    }

    /// <summary>
    /// Lida com a saida do jogador
    /// </summary>
    /// <param name="clientId"></param>
    public void HandlePlayerExit(ulong clientId)
    {
        if (!IsServer) return;

        PlayerData player = GetPlayer(clientId);
        if (player != null)
        {
            players.Remove(player);
        }

        if (roundActive)
        {
            EndRound();
        }

        NotifyOpponentExitClientRpc(clientId);

    }

    /// <summary>
    /// Envia uma carta para um jogador especifico
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="card"></param>
    [ClientRpc]
    private void SendCardClientRpc(ulong clientId, Card card)
    {
        Debug.Log($"Player {clientId} drew: {card.Value} of {card.Suit}");
        Debug.Log($"[CLIENT RPC] Adding card for player {clientId} to UI");

        GameUi.Instance.AddCardToPlayer(clientId, card);
    }

    /// <summary>
    /// Envia uma carta do dealer para todos os clientes
    /// </summary>
    /// <param name="card"></param>
    [ClientRpc]
    private void DealerCardDrawnClientRpc(Card card)
    {
        Debug.Log($"Dealer drew: {card.Value} of {card.Suit}");

        GameUi.Instance.AddCardToDealer(card);
    }

    /// <summary>
    /// Limpa a todos os elementos visuais do jogo (cartas principalmente)
    /// </summary>
    [ClientRpc]
    private void ClearTableClientRpc()
    {
        GameUi.Instance.ClearTable();
    }

    /// <summary>
    /// Mostra a mensagem de vitória para o jogador vencedor
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="reason"></param>
    [ClientRpc]
    private void OnPlayerWinClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU WIN", reason);
    }

    /// <summary>
    /// Mostra a mensagem de derrota para o jogador derrotado
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="reason"></param>
    [ClientRpc]
    private void OnPlayerLoseClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU LOST", reason);
    }

    /// <summary>
    /// Mostra a mensagem de empate (push)
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="reason"></param>
    [ClientRpc]
    private void OnPlayerPushClientRpc(ulong clientId, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        GameUi.Instance.ShowResult("YOU DRAW", reason);
    }

    /// <summary>
    /// Notifica o jogador que permaneceu que o oponente saiu
    /// </summary>
    /// <param name="exitedClientId"></param>
    [ClientRpc]
    private void NotifyOpponentExitClientRpc(ulong exitedClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != exitedClientId)
        {
            GameUi.Instance.ShowResult("Game Ended", "Opponent left the game.");
        }
    }


    /// <summary>
    /// Cliente pede para jogar novemente (o servidor acumula pedidos)
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.Server)]
    public void PlayAgainServerRpc(ulong clientId)
    {
        if (!IsServer) return;

        readyToPlayAgain.Add(clientId);

        // Se ambos os jogadores pediram para jogar novemente inicia nova rodada
        if(readyToPlayAgain.Count == 2 && players.Count == 2)
        {
            StartRound();
        }
    }

    /// <summary>
    /// Cliente pede para sair do jogo
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.Server)]
    public void ExitGameServerRpc(ulong clientId)
    {
        HandlePlayerExit(clientId);
    }
}
