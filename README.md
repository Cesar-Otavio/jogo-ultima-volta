# Última Volta

Jogo de corrida 2D com vista de cima, para PC, feito para a disciplina **Desenvolvimento de Jogos Digitais** (Ciência da Computação, P2).

Corra contra rivais controlados pelo computador em 5 pistas pelo mundo, faça drift, use o nitro e termine no pódio antes do tempo acabar, sem quebrar o carro. Cada corrida dá moedas para gastar na oficina e desafios para completar.

## Integrantes

| Nome | Contribuição principal |
|---|---|
| Amanda Pazold dos Santos | Interface e menus: menu principal, escolha de pista, oficina, desafios, créditos, tela "Sobre", HUD e telas de pausa e resultado |
| Cesar Otavio da Silva Boiani | Coordenação e arquitetura: gerenciador da corrida, cronômetro, recompensas e progresso salvo; versionamento no GitHub, build e entrega |
| Giovana Giraldeli | Arte, áudio e efeitos: sprites e sons (com licenças e créditos), pintura do carro, animações com estados, partículas e fogos de artifício |
| Giovani Nogueira Pires | Física e direção: movimento do carro, drift, nitro, pisos, danos, números da oficina e câmera |
| Marcus Vinicius da Silva Capeteruchi | Pistas e adversários: projeto das 5 pistas, inteligência dos bots, viaturas da polícia, itens da pista, testes de jogo e calibração |

Os scripts de cada área estão listados em [Como o projeto está organizado](#como-o-projeto-está-organizado).

## Como jogar

### Pelo instalador (Windows 64 bits, recomendado)

1. Baixe `UltimaVolta_Instalador_v2.1.exe` na página [Releases](https://github.com/Cesar-Otavio/jogo-ultima-volta/releases) do GitHub. O mesmo arquivo foi entregue no CONNECT+.
2. Abra o instalador e siga os passos. Ele instala só para o seu usuário, sem pedir senha de administrador, e cria um atalho no menu Iniciar (e, se você marcar, na área de trabalho).
3. O jogo abre em tela cheia, no menu. Para sair, use o botão **Sair**.
4. Para desinstalar, use **Configurações > Aplicativos** do Windows.

### Sem instalar

Extraia `UltimaVolta_Jogo_Windows.zip` (também na página Releases) em uma pasta e abra `UltimaVolta.exe`.

O instalador e o executável não têm assinatura digital. Se o Windows mostrar um aviso, clique em **Mais informações** e depois em **Executar assim mesmo**.

Em computadores com o **Controle Inteligente de Aplicativos** do Windows ligado, o Windows pode bloquear o instalador ou o desinstalador por falta de assinatura. Nesse caso, use o ZIP. Para remover o jogo instalado, apague a pasta `%LOCALAPPDATA%\Programs\Ultima Volta` e o atalho do menu Iniciar.

### Pelo projeto na Unity

1. **Só para quem baixou do GitHub, e antes de abrir a Unity:** o repositório público não traz os sprites da CraftPix, porque a licença deles não permite redistribuir os arquivos. O ZIP do projeto entregue no CONNECT+ já vem completo e dispensa este passo.
   1. Baixe os dois pacotes gratuitos (links em [Créditos](#créditos-e-licenças)) e extraia numa pasta.
   2. No PowerShell, dentro da pasta do projeto, rode o comando abaixo, trocando o caminho pela pasta dos pacotes:
      ```
      powershell -ExecutionPolicy Bypass -File tools\importar_sprites_craftpix.ps1 -Pasta "C:\caminho\dos\pacotes"
      ```
   3. O script copia os 98 sprites usados, conferindo o conteúdo de cada um.
   4. Se a Unity já foi aberta sem os sprites, baixe o repositório de novo antes de rodar o script. O menu **Última Volta > Importar sprites da CraftPix...** faz a mesma cópia dentro da Unity.
2. Abra a pasta do projeto no Unity Hub, com a versão **6000.6.7f1**.
   - A primeira abertura importa tudo e pode levar alguns minutos.
   - Se a Unity fechar sozinha durante essa importação, abra o projeto de novo: na segunda vez ela termina.
3. Na janela **Project**, abra `Assets/Scenes` e dê dois cliques em **Menu**.
4. Aperte **Play**. Na corrida, clique uma vez dentro da aba **Game** para o jogo receber o teclado.

## Controles

| Ação | Tecla |
|---|---|
| Acelerar | W ou seta para cima |
| Frear e dar ré | S ou seta para baixo |
| Virar | A e D, ou setas para esquerda e direita |
| Freio de mão (drift) | Barra de espaço |
| Nitro | Shift |
| Pausar | Esc |
| Liberar tudo (modo apresentação, só no menu) | F9 |

## Regras

### Na corrida

- **Objetivo:** completar as voltas antes do tempo acabar, sem quebrar o carro.
- **Colocação:** terminar no pódio (até 3º lugar) libera a próxima pista.
- **Contagem:** a corrida começa com uma contagem 3, 2, 1 no semáforo, e o cronômetro só corre durante a corrida.
- **Barris:** sempre causam dano.
- **Paredes e outros carros:** só causam dano em batidas fortes. Depois de um dano, o carro fica protegido por 1 segundo e pisca.
- **Corações:** o carro aguenta de 3 a 9 batidas, conforme o modelo e a blindagem.
- **Óleo:** faz o carro escorregar.
- **Pisos:** terra escorrega mais; grama, areia e água deixam o carro lento.
- **Rampas:** fazem o carro pular. Na Amazônia, dá para passar por cima do rio.
- **Item HP+:** conserta um dano.
- **Polícia (Nevada):** duas viaturas patrulham devagar. Quem passa rápido perto delas é perseguido por alguns segundos, com sirene.

### Fim da corrida

| Resultado | Quando acontece |
|---|---|
| Colocação final | O jogador cruza a chegada na última volta. O pódio comemora com fogos de artifício. |
| Tempo esgotado | O cronômetro chega a zero. |
| Carro quebrado | Os corações acabam. |

A tela de resultado mostra a classificação, o **ranking da pista** (os 5 melhores tempos já feitos nela), as moedas ganhas, os desafios completos e se uma pista nova foi liberada.

## Pistas

| Pista | País | Voltas | Tempo limite | Destaque |
|---|---|---|---|---|
| Treino | Autódromo | 5 | 3:00 | Circuito original, sem rivais |
| Rio de Janeiro | Brasil | 3 | 4:00 | Orla com prédios e mar, muros na beira da pista |
| Amazônia | Brasil | 3 | 3:45 | Terra e asfalto, floresta, rio com rampa e ponte |
| Nevada | EUA | 3 | 4:15 | Deserto, retas longas, rochas, óleo e polícia |
| Tóquio | Japão | 3 | 4:00 | Corrida noturna com neon e os rivais mais rápidos |

Cada pista tem sua música, com uma versão mais acelerada na última volta.

## Oficina, moedas e desafios

As moedas vêm de três fontes:
- a colocação (de 40 a 150, mais nas pistas avançadas);
- as moedas pegas na pista (5 cada);
- os desafios.

Cada pista tem 3 desafios, como "termine em 1º", "faça 8 s de drift" ou "termine sem nenhum dano". São 15 no total.

Na oficina dá para comprar e trocar:
- **Modelo:** Faísca, Relâmpago (leve) e Blindado (pesado).
- **Pintura:** 9 cores. A dourada só é liberada depois de completar 6 desafios.
- **Som do motor:** 3 opções.
- **Escapamento:** 3 opções.
- **Melhorias:** motor, pneus, freios, blindagem e nitro, cada uma com 3 níveis.

O progresso fica salvo no computador, e **Desafios > Zerar progresso** apaga tudo.

## Requisitos da disciplina

| Pedido | Onde está no jogo |
|---|---|
| Jogo completo, com início, meio e fim | Menu, corridas, telas de resultado |
| Colisão e física | Rigidbody2D nos carros, muros, barris que rolam, batidas entre carros, óleo, pisos, rampas |
| Animação de sprites e objetos com estados | Animator do próprio carro (Parado, Andando, Derrapando), da luz de freio, do nitro e da sirene; sprites de dano; pulo; moedas girando |
| Áudio | 6 músicas, motor que muda com a velocidade, derrapagem, nitro, estouros, batidas, moedas, fogos |
| HUD e menus | Tempo, volta, posição, corações, nitro, velocidade, moedas e minimapa; 7 telas no menu |
| Vitória, game over, reiniciar e voltar | Resultado, pausa, "Tentar de novo", "Próxima pista", "Voltar ao menu" |
| Tela de créditos com as fontes | Menu > Créditos |
| Objetivo claro e personagem controlável | Instruções no menu; carro no teclado |
| Inimigo ou obstáculo com comportamento próprio | Bots que disputam a corrida e viaturas que perseguem |
| Progressão | 5 pistas liberadas em ordem, rivais mais rápidos a cada pista, oficina |
| Extras sugeridos para corrida | Várias pistas e ranking com os 5 melhores tempos de cada pista; também desafios e oficina |
| Playtesting e balanceamento | Testes automáticos em todas as pistas e teste manual (ver [Testes](#testes)) |

## Como o projeto está organizado

```
Assets/
  Animacoes/  controladores de animação (luz de freio, nitro, sirene)
  Audio/      músicas, efeitos e CREDITOS.txt
  Dados/      catálogo, pistas e modelos de carro (ScriptableObjects)
  Editor/     ferramentas que montam as cenas e os prefabs
  Materiais/  shaders do carro, da pista e dos efeitos
  Pistas/     malhas geradas das pistas
  Prefabs/    carro do jogador e carro dos bots
  Scenes/     Menu, Treino e Pistas/ (Rio, Amazonia, Nevada, Toquio)
  Scripts/    código do jogo, separado por assunto
  Sprites/    sprites usados (CraftPix) e Gerados/ (desenhados por código)
  Texturas/   texturas da pista e das câmeras do minimapa e da oficina
docs/         roteiro da apresentação
tools/        script que recoloca os sprites da CraftPix e o projeto do instalador (Inno Setup)
GDD.md        documento de design simplificado
```

| Pasta e script | Responsabilidade | Área |
|---|---|---|
| `Corrida/RaceManager` | Único lugar que decide o estado da corrida: contagem, corrida, pausa, chegada e derrota | Cesar |
| `Corrida/RaceTimer` | Cronômetro regressivo e tempo de prova | Cesar |
| `Corrida/RaceRewards` | Moedas, desafios, recordes e desbloqueio da próxima pista | Cesar |
| `Dados/SaveSystem`, `SaveData` | Progresso salvo em JSON | Cesar |
| `Dados/GameCatalog`, `TrackInfo`, `CarModel` | Dados das pistas, carros e peças | Cesar e Giovani |
| `Carro/CarController` | Movimento, drift, nitro, pisos e pulo, para jogador e bots | Giovani |
| `Carro/PlayerDriver` | Lê o teclado | Giovani |
| `Carro/CarDamage` | Danos, sprite amassado e conserto | Giovani |
| `Carro/CarSetup` | Monta o carro com o modelo, a pintura e as peças da oficina | Giovani |
| `Efeitos/CameraFollow` | Câmera que segue, antecipa, afasta o zoom e treme nas batidas | Giovani |
| `Carro/CarEffects`, `CarSounds` | Marcas de pneu, fumaça, chamas, luzes com Animator, sons do carro | Giovana |
| `Corrida/AudioManager` | Música da pista e efeitos gerais | Giovana |
| `Efeitos/Fireworks`, `Spinner` | Fogos de artifício e vitrine girando | Giovana |
| `Materiais/Carro.shader` | Troca a cor do carro sem precisar de outro sprite | Giovana |
| `Pista/TrackPath`, `Carro/RaceCar` | Linha central da pista, voltas, posições e piso | Marcus |
| `Carro/BotDriver`, `PoliceCar` | Pilotos do computador e viaturas da polícia | Marcus |
| `Pista/*` | Óleo, moedas, conserto, rampa, áreas de água, obstáculos | Marcus |
| `Editor/TrackDesigns`, `TrackBuilder` | Projetos das pistas e montagem automática das cenas | Marcus |
| `Interface/*` | Menu, pistas, oficina, desafios, HUD e telas da corrida | Amanda |
| `Editor/MenuBuilder`, `RaceSceneBuilder`, `UIBuilder` | Montagem das telas por código | Amanda |

### Ferramentas de Editor

As cenas e os prefabs são montados por scripts. Para reconstruir tudo, use o menu **Última Volta**, nesta ordem:

1. Preparar assets
2. Criar prefabs dos carros
3. Converter a pista de Treino
4. Construir pistas novas
5. Construir o menu
6. Gerar executável (com nome, versão e ícone)

O item **Reconstruir tudo e gerar executável** faz os seis passos de uma vez. O instalador é gerado depois, abrindo `tools/instalador/UltimaVolta.iss` no [Inno Setup](https://jrsoftware.org/isinfo.php) e clicando em **Compile**.

Para mudar o traçado de uma pista, edite `Assets/Editor/TrackDesigns.cs` e rode o passo 4.

O jogo não usa variáveis `static` com estado: os scripts se ligam por campos no Inspector. Reiniciar é carregar a cena de novo.

## Testes

- **Corridas automáticas:** um piloto automático correu uma prova completa em cada uma das 5 pistas, em velocidade 4x.
  - Todos os carros terminaram e nenhum bot ficou preso.
  - O Console ficou sem erros.
  - Com o carro inicial, sobraram de 62 a 67 s em cada pista; os tempos limite foram calibrados com isso.
- **Mecânicas medidas:**
  - A rampa da Amazônia faz o carro pular o rio a cada volta.
  - As viaturas de Nevada ligam a sirene e perseguem.
  - O som da largada tem bipes em 0, 1, 2 e 3 s, junto com o 3, 2, 1, VAI da tela.
  - O Animator do carro passa por Parado (na largada), Andando e Derrapando (no drift).
  - O ranking da pista grava o tempo novo na posição certa.
- **Instalador:** instalado em modo silencioso, o jogo instalado abriu sem erros e o desinstalador removeu tudo, inclusive o atalho.
- **Telas:** fotografadas uma a uma: menu, pistas, oficina, desafios, instruções, créditos, sobre e HUD de cada pista.
- **Teste manual:** um integrante jogou o executável e testou menu, oficina, todas as pistas, pausa, resultado e desafios.
- **Testes da versão básica:** os 14 testes continuam documentados no histórico (tag `v1.0-basico`).

## Créditos e licenças

### Sprites

CraftPix.net, pacotes gratuitos:

- [Free Racing Game Kit](https://craftpix.net/freebies/free-racing-game-kit/)
- [Free Race Track 2D Game Tile Set](https://craftpix.net/freebies/free-race-track-2d-game-tile-set/)

A licença está na [seção Freebie Products das CraftPix File Licenses](https://craftpix.net/file-licenses/), conferida em 04/11/2026:
- Permite usar os sprites em jogos e distribuir o jogo, sem exigir crédito.
- **Não permite** redistribuir os arquivos de arte em si. Por isso o repositório público não inclui os PNGs. A lista do que foi usado, com o hash de cada arquivo, está em `Assets/Editor/CraftPix/sprites_craftpix.txt`.

A moeda, os círculos e a marca de pneu foram desenhados por código no próprio projeto.

### Sons

| Som | Autor | Origem | Licença |
|---|---|---|---|
| Músicas: Pure Raceway, Sunny City, Joyful Jungle, Hot Roadway [Remake], Neon sign Circuit [Remake], Racing Game Menu | MintoDog | OpenGameArt.org | CC0 |
| Vitória e derrota (Racing Game Finish & Jingle) | MintoDog | OpenGameArt.org | CC0 |
| Motor (racing car engine sound loops) | domasx2 | OpenGameArt.org | CC0 |
| Batidas (Impact Sounds) | Kenney | kenney.nl | CC0 |
| Fogos e estouros (25 CC0 bang / firework SFX) | rubberduck | OpenGameArt.org | CC0 |
| Moeda, compra e erro (Level up, power up, Coin get) | wobbleboxx | OpenGameArt.org | CC0 |
| Nitro (Boost or Launch or Thruster) | EZduzziteh | OpenGameArt.org | CC0 |
| Largada (Race Start Countdown) | kheetor | OpenGameArt.org | CC0 |
| Derrapagem (Car tire squeal skid loop) | audible-edge (Tom Haigh) | OpenGameArt.org | [CC-BY 3.0](https://creativecommons.org/licenses/by/3.0/) |

Os links de cada som estão em `Assets/Audio/CREDITOS.txt`. Os mesmos créditos aparecem no jogo, em **Menu > Créditos**.

### Ferramentas

- Unity 6000.6.7f1, com Universal Render Pipeline (2D Renderer), Input System e TextMesh Pro.
- Fonte Liberation Sans, incluída no TextMesh Pro (SIL Open Font License).
- Desenvolvido com o apoio do assistente de programação Claude, da Anthropic. A disciplina permite o uso de IA no desenvolvimento.
