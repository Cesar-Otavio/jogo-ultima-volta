# Roteiro da apresentação: Última Volta

**Formato:** duas pessoas e até **15 minutos**.
- O **apresentador** fala.
- O **jogador** fica no computador, joga e abre o código quando o apresentador pedir.

A tabela de cada parte mostra, na mesma linha, o que é falado e o que aparece na tela.

## Antes da aula (jogador)

- [ ] Instalar o jogo no computador da sala com `UltimaVolta_Instalador_v2.1.exe` (ou levar o notebook com o jogo instalado).
  - Se o Windows avisar, clicar em **Mais informações** e depois em **Executar assim mesmo**.
- [ ] Abrir o jogo uma vez e testar o som da sala.
- [ ] No menu, apertar **F9**: libera todas as pistas e dá 5000 moedas para a demonstração.
- [ ] Deixar o GitHub aberto no navegador, numa aba: https://github.com/Cesar-Otavio/jogo-ultima-volta
  - Arquivos para mostrar: `Assets/Scripts/Corrida/RaceManager.cs`, `Assets/Scripts/Carro/CarController.cs`, `Assets/Scripts/Carro/BotDriver.cs` e `Assets/Editor/TrackDesigns.cs`.
- [ ] Plano B: se o computador da sala falhar, usar o notebook ou mostrar o código pelo GitHub.

## 1. Abertura (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "Somos Amanda, Cesar, Giovana, Giovani e Marcus. Este é o **Última Volta**, um jogo de corrida 2D com vista de cima, feito em Unity com C#." | Deixa o **menu** na tela, com o carro girando na vitrine. |
| "O objetivo é completar as voltas antes do tempo acabar, sem quebrar o carro, e terminar no pódio para liberar a próxima pista. São 5 pistas: Treino, Rio de Janeiro, Amazônia, Nevada e Tóquio." | Abre **Jogar** e passa o mouse pelos cartões das pistas. |

## 2. O jogo funcionando (7 min)

### Treino: controles e drift (1 min 30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "O jogador controla o carro com W, A, S e D." | Escolhe **Treino**. Na largada, o carro treme com o motor ligado. |
| "O carro tem animações com estados no Animator: parado, andando e derrapando. A luz de freio e a chama do nitro também têm estados." | Acelera, freia (a luz de freio acende) e acelera de novo. |
| "Segurando Espaço na curva, o carro derrapa: aparecem marca de pneu, fumaça e o som do pneu. O drift enche a barra de nitro." | Faz um **drift** numa curva. |
| "Com Shift, o nitro acelera o carro. Ao soltar o acelerador em alta velocidade, o escapamento estoura." | Usa o **nitro** numa reta. |

### Rio de Janeiro: rivais, HUD e física (2 min 30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "A largada tem semáforo e contagem com som." | Aperta **Esc**, **Voltar ao menu** e escolhe **Rio de Janeiro**. |
| "Corremos contra 3 rivais controlados pelo computador. Eles seguem a pista, freiam antes das curvas, desviam e usam nitro." | Corre e tenta ultrapassar. |
| "No HUD ficam o tempo, a volta, a posição, as moedas, a velocidade, o nitro, os corações e o minimapa." | Pega algumas **moedas**. |
| "Barris sempre causam dano. Paredes e carros, só em batida forte. A cada dano, o carro amassa e pisca." | Bate de leve num **barril** ou num rival. |
| "O Esc pausa tudo: tempo, física e som." | Aperta **Esc**, mostra a pausa, clica **Continuar**. |

### Oficina (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "As corridas e os desafios dão moedas para a oficina. Dá para trocar o modelo, a pintura, o som do motor e o escapamento, e melhorar motor, pneus, freios, blindagem e nitro." | **Esc**, **Voltar ao menu**, **Oficina**. Troca a pintura e o modelo e clica **Ouvir** em dois motores. Compra uma melhoria. |

### Nevada e Tóquio (1 min 30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "Em Nevada há duas viaturas da polícia. Elas patrulham devagar; se o jogador passa rápido perto, ligam a sirene e perseguem." | Corre em **Nevada** e passa perto de uma viatura. |
| "Tóquio é à noite, com faróis e neon, e tem os rivais mais rápidos. A dificuldade sobe a cada pista." | Volta ao menu e corre um trecho de **Tóquio**. |

### Resultado, desafios e créditos (30 s)

| Apresentador fala | Jogador faz |
|---|---|
| "No fim aparecem a classificação, as moedas, os desafios e o ranking com os 5 melhores tempos da pista. No pódio, tem fogos." | Mostra uma **tela de resultado**. Se não der tempo de terminar uma corrida, pula esta linha. |
| "Cada pista tem 3 desafios. A tela de créditos traz a fonte de todos os sprites e sons, e a tela Sobre traz a disciplina e o grupo." | Abre **Desafios**, **Créditos** e **Sobre**. |

## 3. Requisitos da disciplina (1 min)

| Apresentador fala | Jogador faz |
|---|---|
| "O jogo tem início, meio e fim, vitória e derrota, reiniciar e voltar ao menu. Usa física com Rigidbody2D: batidas, barris que rolam, drift e pisos. Tem animação com estados, áudio, HUD, menus e tela de créditos. Os inimigos têm comportamento próprio, os bots e a polícia. A progressão vem das pistas liberadas em ordem, e o extra é o ranking de tempos." | Abre o **README** no GitHub, na tabela "Requisitos da disciplina". |

## 4. Código-fonte (4 min)

| Apresentador fala | Jogador faz |
|---|---|
| "O código está separado por assunto: Carro, Corrida, Dados, Efeitos, Interface e Pista." | Mostra a pasta `Assets/Scripts` no GitHub. |
| "O **RaceManager** é o único lugar que decide o estado da corrida: contagem, correndo, pausado, terminou ou perdeu. Os outros scripts só avisam o que aconteceu." | Abre `RaceManager.cs` e desce até o `enum RaceState` e a função `LapCompleted`. |
| "O **CarController** move o jogador e os bots com o mesmo código. A velocidade é separada em frente e lado; o drift é a aderência lateral baixa." | Abre `CarController.cs`, na função `UpdateLateralSpeed`. |
| "O **BotDriver** mira um ponto à frente na linha central da pista e mede quanto ela vai virar para frear antes da curva." | Abre `BotDriver.cs`, na função `FixedUpdate`. |
| "As pistas são listas de pontos. Uma ferramenta de Editor gera a pista, os muros, os itens e a decoração a partir delas." | Abre `TrackDesigns.cs`, na pista do Rio. |

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

## Perguntas prováveis

- **Por que os sprites não estão no GitHub?** A licença gratuita da CraftPix deixa usar no jogo, mas não redistribuir os arquivos. O projeto completo foi entregue pelo CONNECT+, e o GitHub tem um script que recoloca os sprites.
- **Como a volta é contada?** Pela distância percorrida na linha central da pista. A volta só vale depois de passar pelos 8 setores em ordem, então atalho e ré não contam.
- **Como os bots não ficam muito longe?** Eles ajustam o ritmo pela distância até o jogador: até 15% mais devagar ou 8% mais rápido.
- **Usaram IA?** Sim, um assistente de programação, como a disciplina permite. Isso está escrito no README.
