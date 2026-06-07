# Blackjack Multiplayer com Unity Netcode for GameObjects

## Introdução

Este é o meu projecto para a disciplina de Sistemas de Redes para Jogos, consiste de um jogo de Blackjack 2d em unity para dois jogadores utilizando **Netcode for GameObjects**
para lidar com sistema de redes. A arquitetura é de **Autoridade total do servidor**, ou seja, qualquer calculo ou ação lógica no jogo é feita a nivel do servidor, o cliente apenas 
lida com o UI. O jogo funciona com um sistema **Cliente-Servidor** em vez de Host-Client, pois não só foi o sistema que o professor da cadeira usou durante as aulas como também foi 
o sistema que me pareceu mais interessante de implementar.

## Scripts

|Nome | Função/Descrição |
|------|--------|
|Card.cs| Struct que representa a parte lógica das cartas do jogo, contem o seu valor e naipe, serializável em rede com INetworkSerializable|
|CardView.cs| Classe que trata da componente visual das cartas, dependendo do valor e naipe defenido em Card.cs associa um sprite adequado|
|Deck.cs| Classe que representa a lógica do baralho, inicializa 52 cartas embaralha usando Fisher-Yates e permite sacar cartas|
|Hand.cs| Classe que representa a lógica da mão (grupo de cartas na posse da entiedade (jogador/dealer)) verifica se acontece bust e blackjack|
|Player.cs| NetworkBehaviour anexado ao prefab do jogador. Contém ServerRpc para hit, stand e exit. apenas afetam o cliente que os chamou|
|PlayerData.cs| Classe que representa os dados de cada jogador no servidor, contem ID, mão, e boolean de stand|
|GameUi.cs| Classe singleton que trata do UI do cliente, exibe cartas, textos com os valores das pontuações, botões, indicadores de turno e tela de fim de jogo. Reage a NetworkVariable.OnValueChanged e chama Rpcs para pedir permissão ao servidor por parte do cliente|
|GameManager.cs| NetworkBehaviour principal do projecto, um singleton que gerencia o fluxo do jogo, turnos, NetworkVariables, e Rpcs, maior parte das ações do servidor são feitas neste script|

## Técnicas utilizadas

- **Netcode for GameObjects**:
    - NetworkVariable para sincronizar o estado do jogo
    - ServerRpc e ClientRpc para comunicar entre Server e cliente, sendo que como é de **Autoridade total do servidor** seria sempre para pedir permissão ao servidor e mandar as informações para todos os clientes
    - NetworkBehaviour e NetworkObject para componentes de rede (retirados/inspirados nos utilizados nas aulas)
    - INetworkSerializable ns struckt de Card para facilitar um pouco a minha vida
- **Padrão Singleton**: 
    - GameManager.cs é um singleton
    - GameUi.cs é um singleton
- **Algoritmo de Fisher-Yates**:
    - Este algoritmo foi me recomendado por um colega fora do curso, é possivel que tenha sido falado nas aulas também.
    - O algoritmo consiste de uma maneira eficiente para embaralhar que não é tão dependente do random basico do unity quanto outros metodos
    - Começa-se com o ultimo elemento da lista e define-se esse indice como N-1, a seguir gera-se um indice I aleatório entre x e o indice atual (inclusivo), Reduz-se o indice atual em 1 até chegar a 0


## Bibliografia

- [Videos das Aulas do professor Diogo Andrade](https://www.youtube.com/@diogoandrade9588)
- [Unity Netcode for GameObjects Documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.12/manual/index.html)
- [COMPLETE Unity Multiplayer Tutorial (Netcode for Game Objects): Code Monkey](https://youtu.be/3yuBOB3VrCk?si=LeFLFHHggXOSSYkh)
- [How to use RPCs in Netcode - Unity 6 Tutorial - Part 4: Sunny Valley Studio](https://youtu.be/c6r1yzZRUzQ?si=ofGxPg_Ah5wKuXYF)
- [Getting started with networking concepts: Unity](https://youtu.be/kVt0I6zZsf0?si=FwfkuWIOcx1K2mxe)
- [Fisher–Yates shuffle in C#: Stack Overflow](https://stackoverflow.com/questions/56378647/fisher-yates-shuffle-in-c-sharp)


## Diagrama de arquitectura de redes

Lembrando que o sistema segue uma arquitetura **Cliente-Servidor** com **Autoridade total do servidor**

```mermaid
flowchart TD
    subgraph Servidor["Servidor (Autoridade)"]
        NM[NetworkManager]
        GM[GameManager<br>NetworkBehaviour]
        Deck[Deck]
        PlayersData[players: List<PlayerData>]
        DealerHand[dealerHand: Hand]
        NV[NetworkVariables<br>CurrentTurn, CurrentPlayerIndex,<br>Player1Id, Player2Id,<br>HandValues]
    end

    subgraph ClienteA["Cliente A"]
        PA[Player (NetworkBehaviour)<br>OwnerClientId = A]
        UIA[GameUi]
        CardViewA[CardView]
    end

    subgraph ClienteB["Cliente B"]
        PB[Player (NetworkBehaviour)<br>OwnerClientId = B]
        UIB[GameUi]
        CardViewB[CardView]
    end

    %% Conexões de rede (linhas pontilhadas)
    Servidor <-->|"RPCs (ServerRpc / ClientRpc)"| ClienteA
    Servidor <-->|"RPCs (ServerRpc / ClientRpc)"| ClienteB

    %% Relações internas
    GM --> Deck
    GM --> PlayersData
    GM --> DealerHand
    GM --> NV
    NM -.->|Spawn| PA
    NM -.->|Spawn| PB

    %% Comunicação específica
    PA -->|HitServerRpc, StandServerRpc| GM
    PB -->|HitServerRpc, StandServerRpc| GM
    GM -->|ClientRpc: SendCard, DealerCardDrawn, etc.| UIA
    GM -->|ClientRpc: SendCard, DealerCardDrawn, etc.| UIB
    UIA --> CardViewA
    UIB --> CardViewB

    %% Legenda
    style Servidor fill:#2d4a6e,stroke:#0f172a,stroke-width:2px,color:#fff
    style ClienteA fill:#2d6e4a,stroke:#0f172a,stroke-width:2px,color:#fff
    style ClienteB fill:#2d6e4a,stroke:#0f172a,stroke-width:2px,color:#fff
    linkStyle default stroke-width:2px,stroke:#60a5fa
```
