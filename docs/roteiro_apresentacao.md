# Roteiro da apresentação: Última Volta

Para **uma pessoa** apresentar, em até **15 minutos**: o jogo, o código e a contribuição de cada integrante.

## Antes da aula

- [ ] Copiar a pasta do jogo (`UltimaVolta_Jogo_Windows.zip`, já extraída) para o computador da sala, ou levar o notebook.
- [ ] Abrir o jogo uma vez antes, para o Windows liberar o executável: **Mais informações** e depois **Executar assim mesmo**.
- [ ] Ligar o som da sala e testar o volume no menu.
- [ ] No menu, apertar **F9** para liberar todas as pistas e ganhar 5000 moedas.
- [ ] Deixar o projeto aberto na Unity, ou o GitHub no navegador, para mostrar o código.
  - Arquivos para deixar abertos em abas: `RaceManager.cs`, `CarController.cs`, `BotDriver.cs`, `TrackDesigns.cs` e o Animator `Nitro.controller`.
- [ ] Plano B: se o computador da sala travar, mostrar o vídeo ou as fotos do jogo e o código pelo GitHub.

## 1. Abertura (1 min)

> "Somos Amanda, Cesar, Giovana, Giovani e Marcus. Nosso jogo é o **Última Volta**, uma corrida 2D com vista de cima, feita em Unity 6 com C#. Você corre contra rivais em 5 pistas pelo mundo e precisa terminar no pódio antes do tempo acabar, sem quebrar o carro."

Mostrar o **menu**: o carro girando na vitrine e os botões.

## 2. Jogo funcionando (6 min)

1. **Instruções (20 s):** mostrar a tela rapidamente.
   > "Aqui estão o objetivo e os controles: W A S D, Espaço para drift, Shift para nitro."
2. **Treino (1 min):** dar uma volta.
   - Fazer um drift na curva e mostrar as marcas de pneu, a fumaça e a barra de nitro enchendo.
   - Usar o nitro: a chama azul é uma **animação com estados**.
   - Frear: a luz de freio acende.
3. **Rio de Janeiro (2 min):** uma volta.
   - O semáforo 3-2-1 e o som da largada.
   - Os **3 rivais** e a posição mudando no HUD.
   - As moedas, um barril (dano, o carro amassa e pisca) e o minimapa.
   - Apertar **Esc**: a pausa para tempo, física e som.
   - **Voltar ao menu.**
4. **Oficina (1 min):**
   - Trocar a pintura e o modelo, com a vitrine mudando na hora.
   - Usar **Ouvir** em dois motores.
   - Comprar uma melhoria.
   > "O dinheiro vem das corridas e dos desafios."
5. **Nevada ou Tóquio (1 min 30 s):**
   - Nevada: passar perto da viatura, que liga a sirene e persegue.
   - Tóquio: mostrar a corrida noturna, com faróis e neon.
   - Deixar chegar ao fim (ou reiniciar) e mostrar a **tela de resultado**, com classificação, moedas e desafios. No pódio, os fogos de artifício.
6. **Desafios, Créditos e Sobre (20 s):**
   > "A tela de créditos traz as fontes de todos os sprites e sons. A tela Sobre traz a disciplina e o grupo."

## 3. Requisitos da disciplina (1 min)

Mostrar rapidamente a tabela "Requisitos da disciplina" do README, ou falar:

> "Tem início, meio e fim. Usa física com Rigidbody2D: batidas, barris, drift e pisos. Tem animações com estados no Animator: freio, nitro e sirene. Tem áudio, HUD e menus, vitória e derrota, e reiniciar. Tem tela de créditos. Os inimigos têm comportamento próprio: bots que disputam a corrida e viaturas que perseguem. A progressão vem das 5 pistas liberadas em ordem, com rivais mais rápidos."

## 4. Código (4 min)

1. **`RaceManager` (1 min):**
   > "É o único lugar que decide o estado da corrida: contagem, correndo, pausado, terminou ou perdeu. Os outros scripts só avisam o que aconteceu."
   - Mostrar o enum `RaceState` e a função `LapCompleted`.
2. **`CarController` (1 min):**
   > "O mesmo código move o jogador e os bots. A velocidade é separada em frente e lado. O drift é a aderência lateral baixa: o carro gira mais rápido do que anda."
   - Mostrar `UpdateLateralSpeed`.
3. **`BotDriver` (1 min):**
   > "O bot mira um ponto à frente na linha central da pista e mede quanto a pista vira, para frear antes da curva. Também desvia de carros e se recupera quando bate."
4. **Animator `Nitro` (30 s):** mostrar os 4 estados e as transições pelo parâmetro "Ativo".
5. **`TrackDesigns` (30 s):**
   > "As pistas são listas de pontos. Uma ferramenta de Editor gera a malha da pista, os muros, os itens e a decoração. Mudar a pista é mudar os pontos."

## 5. Contribuição de cada integrante (2 min)

| Integrante | O que falar |
|---|---|
| Amanda | Fez a interface: menu, escolha de pista, oficina, desafios, créditos, HUD, pausa e resultado. |
| Cesar | Coordenou o grupo, fez a arquitetura da corrida (RaceManager, cronômetro, recompensas e progresso salvo), cuidou do GitHub, do build e da entrega. |
| Giovana | Cuidou da arte e do áudio (escolha, licenças e créditos), da pintura do carro, das animações com estados e dos efeitos (fumaça, faíscas, fogos). |
| Giovani | Programou a física e a direção: drift, nitro, pisos, danos, números da oficina e a câmera. |
| Marcus | Projetou as 5 pistas e fez os bots, as viaturas, os itens da pista, os testes de jogo e o balanceamento dos tempos. |

## 6. Encerramento (30 s)

> "O projeto está no GitHub, com README, GDD e instruções. O executável e o projeto completo foram entregues no CONNECT+. Obrigado!"

Abrir para perguntas, se houver tempo.

## Perguntas prováveis

- **Por que os PNGs não estão no GitHub?** A licença gratuita da CraftPix permite usar os sprites no jogo, mas não redistribuir os arquivos. O projeto completo foi entregue pelo CONNECT+. Quem baixa do GitHub usa o menu **Última Volta > Importar sprites da CraftPix**.
- **Como a volta é contada?** Pela distância percorrida na linha central da pista. A volta só vale depois de passar pelos 8 setores em ordem, então atalho e ré não contam.
- **Como os bots não ficam muito longe?** Eles ajustam o ritmo conforme a distância para o jogador (até 15% mais devagar, ou 8% mais rápido).
- **Usaram IA?** Sim, um assistente de programação, como a disciplina permite. O README informa isso.
