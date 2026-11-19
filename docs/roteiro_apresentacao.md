# Roteiro da apresentação: Última Volta

**Formato:** duas pessoas e até **15 minutos**.
- O **apresentador** fala.
- O **jogador** fica no computador, joga e abre o código quando o apresentador pedir.

Nas partes 1 a 3 e 5 e 6, cada linha da tabela mostra, na mesma linha, o que é falado e o que aparece na tela. A parte 4 (código) é dividida em blocos, explicados lá.

| Parte | Tempo |
|---|---|
| 1. Abertura | 1 min |
| 2. O jogo funcionando | 5 min 30 s |
| 3. Requisitos da disciplina | 30 s |
| 4. Código-fonte | 6 min |
| 5. Contribuições | 1 min 30 s |
| 6. Encerramento | 30 s |

## Antes da aula (jogador)

- [ ] Instalar o jogo no computador da sala com `UltimaVolta_Instalador_v2.1.exe` (ou levar o notebook com o jogo instalado).
  - Se o Windows avisar, clicar em **Mais informações** e depois em **Executar assim mesmo**.
- [ ] Abrir o jogo uma vez e testar o som da sala.
- [ ] No menu, apertar **F9**: libera todas as pistas e dá 5000 moedas para a demonstração.
- [ ] Deixar o GitHub aberto no navegador, com uma aba para cada arquivo da parte 4, na ordem:
  1. `Assets/Scripts` (a pasta)
  2. `Assets/Scripts/Corrida/RaceManager.cs`
  3. `Assets/Scripts/Carro/CarController.cs`
  4. `Assets/Scripts/Carro/RaceCar.cs`
  5. `Assets/Scripts/Carro/BotDriver.cs`
  6. `Assets/Scripts/Carro/CarDamage.cs`
  7. `Assets/Scripts/Carro/CarEffects.cs`
  8. `Assets/Editor/TrackDesigns.cs`
- [ ] **Dica do GitHub:** para pular direto para uma linha, acrescente `#L` e o número no fim do endereço. Por exemplo, `.../RaceManager.cs#L60` destaca a linha 60.
- [ ] Plano B: se o computador da sala falhar, usar o notebook ou mostrar o código pelo GitHub.

## 1. Abertura (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "Somos Amanda, Cesar, Giovana, Giovani e Marcus. Este é o **Última Volta**, um jogo de corrida 2D com vista de cima, feito em Unity com C#." | Deixa o **menu** na tela, com o carro girando na vitrine. |
| "O objetivo é completar as voltas antes do tempo acabar, sem quebrar o carro, e terminar no pódio para liberar a próxima pista. São 5 pistas: Treino, Rio de Janeiro, Amazônia, Nevada e Tóquio." | Abre **Jogar** e passa o mouse pelos cartões das pistas. |

## 2. O jogo funcionando (5 min 30 s)

### Treino: controles e drift (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "O jogador controla o carro com W, A, S e D. Na largada, o carro fica no estado Parado do Animator: o motor ligado faz ele tremer." | Escolhe **Treino** e espera a contagem. |
| "Segurando Espaço na curva, o carro derrapa: aparecem marca de pneu, fumaça e o som do pneu, e a barra de nitro enche. Com Shift, o nitro acelera; ao frear, a luz de freio acende." | Faz um **drift**, usa o **nitro** e freia. |

### Rio de Janeiro: rivais, HUD e física (2 min)

| Apresentador fala | Jogador faz |
|---|---|
| "A largada tem semáforo e contagem com som." | Aperta **Esc**, **Voltar ao menu** e escolhe **Rio de Janeiro**. |
| "Corremos contra 3 rivais controlados pelo computador. Eles seguem a pista, freiam antes das curvas, desviam e usam nitro." | Corre e tenta ultrapassar. |
| "No HUD ficam o tempo, a volta, a posição, as moedas, a velocidade, o nitro, os corações e o minimapa." | Pega algumas **moedas**. |
| "Barris sempre causam dano. Paredes e carros, só em batida forte. A cada dano, o carro amassa e pisca." | Bate de leve num **barril** ou num rival. |
| "O Esc pausa tudo: tempo, física e som." | Aperta **Esc**, mostra a pausa, clica **Continuar**. |

### Oficina (45 s)

| Apresentador fala | Jogador faz |
|---|---|
| "As corridas e os desafios dão moedas para a oficina. Dá para trocar o modelo, a pintura, o som do motor e o escapamento, e melhorar motor, pneus, freios, blindagem e nitro." | **Esc**, **Voltar ao menu**, **Oficina**. Troca a pintura e o modelo e clica **Ouvir** num motor. |

### Nevada e Tóquio (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "Em Nevada há duas viaturas da polícia. Elas patrulham devagar; se o jogador passa rápido perto, ligam a sirene e perseguem." | Corre em **Nevada** e passa perto de uma viatura. |
| "Tóquio é à noite, com faróis e neon, e tem os rivais mais rápidos. A dificuldade sobe a cada pista." | Volta ao menu e corre um trecho de **Tóquio**. |

### Resultado, desafios e créditos (45 s)

| Apresentador fala | Jogador faz |
|---|---|
| "No fim aparecem a classificação, as moedas, os desafios e o ranking com os 5 melhores tempos da pista. No pódio, tem fogos." | Mostra uma **tela de resultado**. Se não der tempo de terminar uma corrida, pula esta linha. |
| "Cada pista tem 3 desafios. A tela de créditos traz a fonte de todos os sprites e sons, e a tela Sobre traz a disciplina e o grupo." | Abre **Desafios**, **Créditos** e **Sobre**. |

## 3. Requisitos da disciplina (30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "Atendemos tudo o que foi pedido: início, meio e fim; vitória e derrota; física com Rigidbody2D; animação com estados; áudio; HUD e menus; tela de créditos; inimigos com comportamento próprio, que são os bots e a polícia; e progressão por pistas. Como extra de jogo de corrida, fizemos várias pistas e o ranking." | Abre o **README** no GitHub, na tabela "Requisitos da disciplina". |

## 4. Código-fonte (6 min)

Cada bloco diz **o que o jogador abre**, **o que o apresentador fala** e **o trecho de código** que aparece na tela.

### 4.1 Como o projeto está organizado (30 s)

**Jogador abre:** a pasta `Assets/Scripts` no GitHub.

**Apresentador fala:**
> "São 35 scripts do jogo e 11 ferramentas de Editor, com cerca de 6.400 linhas de C#. O código está separado por assunto: **Carro**, **Corrida**, **Dados**, **Efeitos**, **Interface** e **Pista**. Cada script tem uma responsabilidade só. Os nomes estão em inglês, como é comum em programação, e os comentários em português, explicando o porquê de cada parte."

> "O caminho de um comando é este: o teclado ou a inteligência do bot decide o que fazer, o CarController move o carro com a física da Unity, o RaceCar acompanha em que ponto da pista o carro está, e o RaceManager decide o estado da corrida. As telas, o áudio e o salvamento só consultam o RaceManager."

```
Teclado ── PlayerDriver ─┐
                          ├─► CarController ─► Rigidbody2D (física da Unity)
Pista ──── BotDriver ─────┘
RaceCar (voltas e posição) ─► RaceManager (estado da corrida)
                                   ├─► HUD, telas e áudio (só consultam)
                                   └─► RaceRewards ─► SaveSystem (arquivo JSON)
```

### 4.2 RaceManager: a máquina de estados da corrida (1 min)

**Jogador abre:** `Assets/Scripts/Corrida/RaceManager.cs`, primeiro na linha **5** e depois nas linhas **60**, **77**, **131** e **203**.

**Apresentador fala:**
> "O RaceManager é o único lugar que decide o estado da corrida. A corrida é uma **máquina de estados** com cinco estados: contagem, correndo, pausado, terminou e perdeu."

```csharp
public enum RaceState { Countdown, Racing, Paused, Finished, Lost }
```

> "No `Update` (linha 60), durante a contagem, o tempo vai diminuindo; quando chega a zero, o estado muda para `Racing` e a corrida começa."

> "A função `CanDrive` (linha 77) responde quem pode acelerar agora. Todos os carros, do jogador e dos bots, perguntam isso antes de se mexer. Assim, ninguém larga antes do semáforo, e depois que o jogador cruza a chegada os bots continuam até o fim."

> "A pausa (linha 131) usa `Time.timeScale = 0`. Isso congela de uma vez a física, o cronômetro e as animações, e `AudioListener.pause` congela os sons."

```csharp
Time.timeScale = paused ? 0f : 1f;
AudioListener.pause = paused;
```

> "Quando a corrida acaba, o `EndRace` (linha 203) junta o resumo da prova, com posição, tempo, danos, moedas, drift e nitro, e entrega para o RaceRewards calcular as recompensas e salvar o progresso. Os outros scripts **avisam** o que aconteceu, por exemplo 'completei uma volta' ou 'o carro quebrou', mas **só o RaceManager muda o estado**. Isso evita, por exemplo, que a tela de vitória e a de derrota apareçam juntas."

### 4.3 CarController: física e drift (1 min 30 s)

**Jogador abre:** `Assets/Scripts/Carro/CarController.cs`, nas linhas **91**, **116**, **144**, **177** e **190**.

**Apresentador fala:**
> "Este script move qualquer carro. O do jogador e os dos bots usam exatamente o mesmo código; a única diferença é quem chama a função `SetInput`: o PlayerDriver, que lê o teclado, ou o BotDriver, que é a inteligência artificial."

> "A física roda no `FixedUpdate` (linha 91), que a Unity chama 50 vezes por segundo, em intervalos fixos. Isso deixa o movimento igual em qualquer computador, rápido ou lento."

> "O ponto principal está nas linhas 116 a 123: a velocidade do carro é separada em duas partes. Usamos o **produto escalar** (`Vector2.Dot`), que mede quanto de um vetor aponta numa direção. Uma parte é a velocidade **para a frente**; a outra é a velocidade **de lado**, que é o carro escorregando."

```csharp
float forwardSpeed = Vector2.Dot(velocity, forward);
float lateralSpeed = Vector2.Dot(velocity, right);
```

> "A velocidade para a frente (linha 144) muda com o acelerador, o freio, a ré, o nitro e o piso: na grama, por exemplo, o limite é 60% da velocidade máxima."

> "O **drift** está na linha 177. A cada passo, a velocidade de lado é multiplicada por um número menor que 1, então o escorregão vai sumindo. Com aderência normal (10), ele some quase na hora. Com o freio de mão, a aderência cai para 1,5, o escorregão dura mais e o carro desliza de lado: é o drift."

```csharp
return lateral * Mathf.Exp(-currentGrip * dt);
```

> "Na linha 190, o carro só vira se estiver andando, e o giro inverte na ré, como num carro de verdade. As batidas são resolvidas pela física da Unity, com Rigidbody2D e colisores. Como lemos a velocidade real da física a cada passo, depois de uma batida o carro já está mais lento, sem precisar de nenhum código extra."

### 4.4 Pista, voltas e posições (1 min)

**Jogador abre:** `Assets/Scripts/Carro/RaceCar.cs`, nas linhas **63**, **69**, **85** e **40**.

**Apresentador fala:**
> "Cada pista tem uma **linha central**: uma lista de pontos, um a cada unidade, no componente TrackPath. A cada passo de física, o RaceCar procura o ponto da linha mais perto do carro, olhando só perto do último ponto conhecido, e calcula quanto o carro já andou na volta (linha 63)."

> "Para a volta contar, a pista é dividida em **8 setores**, e o carro precisa passar por todos **em ordem** (linha 85). Quando ele cruza a chegada com os 8 setores feitos, a volta conta (linha 69). Assim, cortar caminho ou dar ré e cruzar a linha de novo não vale."

> "O progresso de cada carro (linha 40) é: voltas completas vezes o tamanho da pista, mais a distância andada na volta atual. O RaceManager ordena os carros por esse número para mostrar a posição no HUD. Quem já terminou fica na ordem de chegada."

```csharp
float progress = laps * track.Length + lapDistance;
```

### 4.5 BotDriver e polícia: a inteligência artificial (1 min)

**Jogador abre:** `Assets/Scripts/Carro/BotDriver.cs`, nas linhas **36**, **51**, **70**, **83** e **98**.

**Apresentador fala:**
> "O bot não tem um caminho gravado: ele decide a cada passo de física (linha 36). Primeiro, mira um ponto **à frente** na linha central, numa faixa própria (linha 51). Quanto mais rápido ele está, mais longe olha, como um piloto de verdade."

```csharp
float lookAhead = 4f + Mathf.Max(0f, speed) * 0.55f;
```

> "Depois, ele calcula o ângulo entre a frente do carro e esse ponto e vira o volante na proporção desse ângulo."

> "Para não bater nas curvas (linha 70), ele soma quanto a pista vai virar nos próximos metros. Muita curva à frente significa uma velocidade segura menor, então ele freia antes de chegar nela. A habilidade do bot muda essa velocidade de curva: em Tóquio os bots são mais hábeis do que no Rio, e é assim que a dificuldade cresce."

> "Ele também usa nitro nas retas, desvia de um carro logo à frente usando um *CircleCast*, que é um círculo lançado à frente para detectar colisões (linha 83), e dá ré ou volta para a pista se ficar preso. A função `PaceFactor` (linha 98) faz o bot aliviar um pouco quando está muito à frente do jogador e forçar quando está atrás, para a corrida ficar disputada."

> "A **viatura da polícia** (`PoliceCar.cs`, linha 45) usa o mesmo piloto com uma máquina de estados própria: patrulha devagar; se o jogador passa rápido perto, persegue por 4 segundos com a sirene ligada; depois espera 6 segundos antes de perseguir de novo."

### 4.6 Danos e animações com estados (30 s)

**Jogador abre:** `Assets/Scripts/Carro/CarDamage.cs`, nas linhas **46** e **59**. Depois, `Assets/Scripts/Carro/CarEffects.cs`, na linha **114**.

**Apresentador fala:**
> "Na batida (linha 46), medimos só a parte da velocidade que vai **contra** o obstáculo. Bater de frente é forte; raspar de lado é fraco. Barril sempre causa dano; parede e outro carro, só acima de 3 unidades por segundo (linha 59). Depois de um dano, o carro fica 1 segundo protegido e pisca."

```csharp
float impactSpeed = Mathf.Abs(Vector2.Dot(collision.relativeVelocity, contact.normal));
```

> "As **animações com estados** usam o Animator da Unity. O código não troca a animação diretamente: ele só manda **parâmetros**, que são a velocidade, se está derrapando, se está freando e se o nitro está ligado (linha 114). Os estados e as transições ficam no Animator Controller. O carro tem os estados Parado, Andando e Derrapando; a luz de freio, o nitro e a sirene da polícia têm os seus."

```csharp
bodyAnimator.SetFloat("Velocidade", Mathf.Abs(car.ForwardSpeed));
bodyAnimator.SetBool("Derrapando", car.IsDrifting);
```

### 4.7 Dados, progresso e pistas geradas por código (30 s)

**Jogador abre:** `Assets/Editor/TrackDesigns.cs`, na linha **74** (a pista do Rio).

**Apresentador fala:**
> "Os dados do jogo, como pistas, carros, peças e preços, ficam em **ScriptableObjects**: arquivos de dados que dá para editar no Inspector da Unity sem mexer no código. O progresso (moedas, pistas liberadas, desafios e ranking) é salvo num arquivo **JSON** pelo SaveSystem, usando o `JsonUtility` da Unity."

> "As pistas não foram montadas à mão: cada uma é uma lista de pontos, como esta do Rio. Uma ferramenta de Editor que nós fizemos, o TrackBuilder, liga os pontos com uma **curva suave** (Catmull-Rom). A partir dessa curva, ela gera a malha da pista com a textura de asfalto ou de terra repetida, os muros contínuos, os itens e a decoração. Para mudar uma pista, basta mudar os pontos e rodar o menu **Última Volta** da Unity."

## 5. Contribuições (1 min 30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "A Amanda fez a interface: menus, oficina, desafios, HUD e telas da corrida." | Mostra a tabela de integrantes do README. |
| "O Cesar coordenou o grupo e fez a arquitetura da corrida, o salvamento do progresso, o GitHub e o build." | — |
| "A Giovana cuidou da arte, do áudio, das licenças e créditos, das animações e dos efeitos." | — |
| "O Giovani programou a física do carro: drift, nitro, pisos, danos, números da oficina e a câmera." | — |
| "O Marcus projetou as pistas e fez os bots, a polícia, os itens e os testes de equilíbrio." | — |

## 6. Encerramento (30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "O projeto está no GitHub com README, GDD e o instalador na página Releases. Obrigado! Ficamos à disposição para perguntas." | Volta para o **menu** do jogo. |

## Se o tempo apertar

- Na parte 2, pule Nevada e mostre só Tóquio.
- Na parte 4, resuma os blocos 4.6 e 4.7 em uma frase cada:
  - "As batidas medem a força contra o obstáculo, e as animações são estados no Animator."
  - "As pistas são listas de pontos transformadas em pista por uma ferramenta nossa."

## Perguntas prováveis do professor sobre o código

**Por que `FixedUpdate` e não `Update`?**
O `Update` roda uma vez por quadro, e o número de quadros muda de computador para computador. O `FixedUpdate` roda em intervalos fixos (50 vezes por segundo), que é o que a física da Unity usa. O teclado é lido no `Update` (`PlayerDriver`) e aplicado no `FixedUpdate` (`CarController`).

**O que o Rigidbody2D faz?**
É o componente de física da Unity: guarda velocidade, massa e rotação e resolve as colisões com os colisores (`BoxCollider2D` no carro, `EdgeCollider2D` nos muros). Nós controlamos a velocidade, e a Unity cuida das batidas.

**O que é um trigger?**
É um colisor que detecta a passagem sem bloquear o carro. Moedas, óleo, rampas, água e o item de conserto usam trigger, com a função `OnTriggerEnter2D`.

**Por que vocês não usam variáveis `static`?**
O projeto desliga a recarga de scripts ao apertar Play, para testar mais rápido. Com isso, valores `static` não voltam ao início entre uma partida e outra. Por isso os scripts se ligam por campos no Inspector, e reiniciar a corrida é carregar a cena de novo.

**Como funciona o minimapa?**
Uma segunda câmera vê só a camada "Minimapa" (a linha da pista e um círculo colorido por carro) e desenha numa **RenderTexture**, que aparece no HUD como uma imagem.

**Como funciona a troca de cor do carro?**
Com um **shader** próprio (`Assets/Materiais/Carro.shader`). Ele converte a cor de cada pixel para matiz, saturação e brilho (HSV) e gira a matiz só das partes coloridas, sem precisar de um sprite para cada cor. O valor é enviado pelo `CarSetup` com um `MaterialPropertyBlock`.

**Como o progresso é salvo?**
A classe `SaveData` vira texto JSON com `JsonUtility.ToJson` e é gravada num arquivo na pasta de dados do jogo (`Application.persistentDataPath`). Ao abrir o jogo, o `SaveSystem` lê o arquivo de volta.

**Como os bots sabem a hora de frear?**
Somando quanto a direção da linha central muda nos próximos metros (`TrackPath.TurnBetween`). Quanto maior essa soma, menor a velocidade segura.

**Como vocês testaram o jogo?**
Com um piloto automático: o próprio BotDriver colocado no carro do jogador. Ele correu provas completas em todas as pistas em velocidade 4x, e isso calibrou os tempos limite. Também jogamos o jogo manualmente.

**Usaram IA?**
Sim, um assistente de programação, como a disciplina permite. Isso está escrito no README.

## Glossário rápido (para o apresentador)

| Termo | Explicação curta |
|---|---|
| Rigidbody2D | Componente de física: velocidade, massa e colisões |
| Collider / trigger | Forma usada nas colisões / colisor que só detecta, sem bloquear |
| Update / FixedUpdate | Roda a cada quadro / roda em intervalo fixo, para física |
| Produto escalar (`Vector2.Dot`) | Quanto de um vetor aponta na direção de outro |
| Máquina de estados | O objeto está sempre em um estado, e regras dizem quando ele troca |
| Animator / parâmetro / transição | Ferramenta de animação da Unity / valor que o código envia / regra para mudar de estado |
| ScriptableObject | Arquivo de dados editável no Inspector |
| Prefab | Objeto pronto e reutilizável (o carro do jogador e o carro dos bots) |
| RenderTexture | Imagem desenhada por uma câmera (minimapa e vitrine da oficina) |
| `Time.timeScale` | Velocidade do tempo do jogo (0 = pausado) |
| Catmull-Rom | Fórmula que liga pontos com uma curva suave |
