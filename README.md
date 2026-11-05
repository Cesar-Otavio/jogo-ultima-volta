# Última Volta

Jogo de corrida 2D com vista de cima, para PC, feito para a disciplina **Desenvolvimento de Jogos Digitais** (Ciência da Computação, P2).

Complete 5 voltas em até 3 minutos sem quebrar o carro. Cada batida forte deixa o carro visivelmente mais amassado, até ele não aguentar mais.

## Integrantes

| Nome | Contribuição |
|---|---|
| Amanda Pazold dos Santos | [PREENCHER] |
| Cesar Otavio da Silva Boiani | [PREENCHER] |
| Giovana Giraldeli | [PREENCHER] |
| Giovani Nogueira Pires | [PREENCHER] |
| Marcus Vinicius da Silva Capeteruchi | [PREENCHER] |

## Como jogar

### Pelo executável (Windows 64 bits)

1. Extraia o arquivo `UltimaVolta_Jogo_Windows.zip` em uma pasta.
2. Abra `UltimaVolta.exe`. O jogo abre em tela cheia, no menu.
3. Para sair, use o botão **Sair** do menu.

O executável não tem assinatura digital. Se o Windows mostrar um aviso de segurança, clique em **Mais informações** e depois em **Executar assim mesmo**.

### Pelo projeto na Unity

1. Abra a pasta do projeto no Unity Hub, com a versão **6000.6.7f1**.
2. Na janela **Project**, abra `Assets/Scenes` e dê dois cliques em **Menu**. Sem isso, a Unity pode abrir uma cena vazia, e o Play não mostra nada.
3. Aperte **Play**. Na corrida, clique uma vez dentro da aba **Game** para o jogo receber o teclado.

## Controles

| Ação | Tecla |
|---|---|
| Acelerar | W ou seta para cima |
| Frear e dar ré | S ou seta para baixo |
| Virar | A e D, ou setas para esquerda e direita |
| Freio | Barra de espaço |
| Pausar | Esc |

Com a frente do carro reta contra a parede, o acelerador sozinho não tira o carro dali. Dê ré com S ou seta para baixo.

## Regras

- A corrida dura até **3 minutos**. O cronômetro começa em `03:00` e só conta durante a corrida.
- São **5 voltas**. A volta só conta depois de passar pelo ponto de controle, do lado oposto do circuito.
- **Barris** sempre causam dano. **Paredes** só causam dano em batidas fortes: raspar de lado não conta.
- Depois de um dano, o carro fica protegido por 1 segundo. Continuar encostado conta como um dano só.
- O **óleo** faz o carro escorregar por 1,5 segundo. Dá para desviar.

### Vitória

Cruzar a linha de chegada na quinta volta, com tempo restante.

### Derrota

- **Tempo esgotado:** o cronômetro chegou a zero.
- **Carro quebrado:** o carro sofreu 4 danos.

## Como o projeto está organizado

```
Assets/
  Audio/      músicas, efeitos e CREDITOS.txt
  Fisica/     material físico sem atrito do carro
  Scenes/     Menu e Corrida
  Scripts/    código do jogo
  Sprites/    somente os sprites usados (22 de 254)
```

| Script | Responsabilidade |
|---|---|
| `RaceManager` | Único lugar que decide o estado da corrida; pausa, reinicia e volta ao menu |
| `RaceTimer` | Conta o tempo só durante a corrida e avisa quando zera |
| `CarController` | Movimento do carro e efeito do óleo |
| `CarDamage` | Conta danos, troca o sprite e avisa quando o carro quebra |
| `FinishLine` e `LapCheckpoint` | Avisam quando o carro cruza a chegada e o ponto de controle |
| `Obstacle` e `OilSlick` | Marcam os barris e aplicam o efeito do óleo |
| `CameraFollow` | Segue o carro sem mostrar o que está fora do mapa |
| `HudController` | Mostra tempo, volta atual e corações |
| `RaceScreens` e `MenuController` | Telas da corrida e botões do menu |
| `AudioManager` | Música, motor e efeitos |

Mais detalhes de design estão no [GDD](GDD.md).

## Testes

Os 14 testes exigidos passaram no Editor em uma sessão contínua, do menu até o fim, com teclas simuladas. O executável foi aprovado em um teste manual.

| # | Teste | Resultado |
|---|---|---|
| 1 | A corrida inicia e o cronômetro começa em 03:00 | Passou |
| 2 | O carro completa o percurso sem sair da pista | Passou |
| 3 | O carro não atravessa as guias nas curvas | Passou |
| 4 | A câmera não mostra o exterior do mapa | Passou |
| 5 | Colisões válidas aumentam os danos | Passou |
| 6 | O carro muda visualmente a cada dano | Passou |
| 7 | No limite de danos, a corrida termina em derrota | Passou |
| 8 | Com o tempo em zero, a corrida termina em derrota | Passou |
| 9 | Cruzar a chegada a tempo dá vitória | Passou |
| 10 | A pausa interrompe o cronômetro e o movimento | Passou |
| 11 | Reiniciar restaura tempo e danos | Passou |
| 12 | As telas de vitória e derrota nunca aparecem juntas | Passou |
| 13 | Nenhum erro crítico no Console | Passou |
| 14 | O jogo pode ser concluído do começo ao fim sem parar o Editor | Passou |

## Créditos e licenças

### Sprites

CraftPix.net, pacotes gratuitos:

- [Free Racing Game Kit](https://craftpix.net/freebies/free-racing-game-kit/)
- [Free Race Track 2D Game Tile Set](https://craftpix.net/freebies/free-race-track-2d-game-tile-set/)

Licença: [CraftPix File Licenses, seção Freebie Products](https://craftpix.net/file-licenses/), conferida em 04/11/2026. Permite usar os sprites em jogos pessoais e comerciais e distribuir o jogo, sem exigir crédito. **Não permite** revender nem redistribuir os arquivos de arte em si. Por isso, os PNGs deste repositório não devem ser reaproveitados separadamente do jogo.

### Sons

Todos com licença [CC0](https://creativecommons.org/publicdomain/zero/1.0/), conferida nas páginas originais em 04/11/2026:

| Som | Autor | Origem |
|---|---|---|
| Músicas da corrida e da última volta | MintoDog | [Pure Raceway](https://opengameart.org/content/pure-raceway) |
| Sons de vitória e derrota | MintoDog | [Racing Game Finish & Jingle](https://opengameart.org/content/racing-game-finish-jingle) |
| Motor | domasx2 | [Racing car engine sound loops](https://opengameart.org/content/racing-car-engine-sound-loops) |
| Batidas | Kenney | [Impact Sounds](https://kenney.nl/assets/impact-sounds) |

### Ferramentas

- Unity 6000.6.7f1, com Universal Render Pipeline (2D Renderer), Input System e TextMesh Pro.
- Fonte LiberationSans, incluída nos recursos do TextMesh Pro.
- Desenvolvido com o apoio do assistente de programação Claude, da Anthropic. A disciplina permite o uso de IA no desenvolvimento.
