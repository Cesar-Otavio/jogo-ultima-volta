# Última Volta: GDD simplificado

**Grupo:** Amanda Pazold dos Santos, Cesar Otavio da Silva Boiani, Giovana Giraldeli, Giovani Nogueira Pires e Marcus Vinicius da Silva Capeteruchi
**Disciplina:** Desenvolvimento de Jogos Digitais, Ciência da Computação (P2)
**Entrega:** 19/11/2026

## 1. Visão geral

| Item | Descrição |
|---|---|
| Gênero | Corrida arcade 2D |
| Vista | De cima (top-down) |
| Plataforma | PC com Windows (executável) |
| Jogadores | 1, contra até 3 rivais controlados pelo computador |
| Duração da corrida | De 3 a 4 minutos |
| Motor | Unity 6000.6.7f1, URP 2D, C# |

**Conceito:** uma volta ao mundo em corridas curtas. O jogador equilibra velocidade e cuidado: ir rápido, fazer drift e usar o nitro ganha posições, mas cada batida forte amassa o carro até ele quebrar. Entre as corridas, o dinheiro ganho melhora o carro na oficina.

**Objetivo:** completar as voltas antes do tempo acabar, sem quebrar o carro, e terminar no pódio para liberar a próxima pista.

## 2. Ciclo de jogo

1. No menu, o jogador escolhe uma pista liberada, ou passa antes pela oficina.
2. Contagem 3, 2, 1 no semáforo e largada no fim do grid.
3. Corrida: drift nas curvas, nitro nas retas, desvio de barris e óleo, moedas pelo caminho.
4. Resultado: classificação, moedas, desafios completos e pista nova liberada.
5. Oficina: comprar modelo, pintura, som do motor, escapamento e melhorias.
6. A próxima pista tem rivais mais rápidos.

## 3. Mecânicas

### Carro

O carro é um corpo físico (Rigidbody2D). A cada passo de física, a velocidade é separada em duas partes:
- **Para a frente:** muda com acelerador, freio e piso.
- **De lado:** diminui conforme a aderência.

Com pouca aderência (freio de mão, óleo, terra), o carro escorrega: é o drift.

| Modelo | Velocidade máxima | Aceleração | Curva | Corações | Preço |
|---|---|---|---|---|---|
| Faísca | 180 km/h | 8,5 | 175 °/s | 4 | Inicial |
| Relâmpago (leve) | 187 km/h | 10 | 190 °/s | 3 | 450 |
| Blindado (pesado, empurra os rivais) | 176 km/h | 7,5 | 160 °/s | 6 | 700 |

No jogo, 10 unidades por segundo aparecem como 180 km/h.

### Drift e nitro

| Item | Valor |
|---|---|
| Aderência normal | 10 |
| Aderência com freio de mão | 1,5 |
| Giro extra no freio de mão | 40% |
| Nitro | 35% mais velocidade máxima, tanque de 2,5 s |
| Recarga do nitro | 30% do tanque por segundo de drift, mais 2% por segundo sozinho |

O drift conta como deslizamento lateral acima de 1,5 unidade/s, com o carro a mais de 3 unidades/s.

### Pisos

| Piso | Velocidade máxima | Aderência |
|---|---|---|
| Asfalto | 100% | 100% |
| Terra | 92% | 45% |
| Grama | 60% | 60% |
| Areia | 65% | 50% |
| Água | 45% | 40% |

### Danos e itens

| Situação | Efeito |
|---|---|
| Bater em um barril | Sempre causa dano. O barril é empurrado. |
| Bater em parede ou carro a 3 unidades/s ou mais, contando só a velocidade contra o obstáculo | Causa dano |
| Raspar ou bater devagar | Não causa dano |
| Até 1 s depois de um dano | Não causa dano; o carro pisca |
| Item HP+ | Conserta 1 dano |
| Óleo | Escorrega por 1,5 s |
| Rampa | Pula de 0,4 a 0,9 s, conforme a velocidade. No ar, o carro não sente o piso. |
| Moeda | +5 moedas |

O carro tem 5 sprites, do intacto ao quebrado. Os danos são distribuídos entre eles, conforme o número de corações.

### Voltas e posições

Cada pista tem uma linha central, o `TrackPath`. O progresso de cada carro é a distância percorrida ao longo dela.

A volta só conta depois de passar pelos 8 setores da pista em ordem. Isso impede atalhos e ré pela linha de chegada.

A posição é calculada assim:
- quem terminou fica na ordem de chegada;
- os outros carros são ordenados pelo progresso.

Andar ao contrário por mais de 1 s mostra "CONTRAMÃO!".

## 4. Adversários

| Comportamento | Como funciona |
|---|---|
| Seguir a pista | Mira um ponto à frente na linha central, numa faixa própria. O ponto fica mais longe quanto maior a velocidade. |
| Frear antes das curvas | Mede quanto a pista vira à frente e reduz a velocidade. Bots mais hábeis fazem curvas mais rápido. |
| Nitro | Usa o nitro nas retas, se a habilidade for maior que 0,4 |
| Ultrapassar | Desvia para o lado quando há um carro logo à frente |
| Ritmo | Alivia até 15% se está muito à frente do jogador e força até 8% se está atrás. Isso mantém a corrida disputada. |
| Recuperação | Dá ré se fica parado contra algo. Se fica preso por 5 s, volta para a pista. |

**Viaturas da polícia (Nevada):**
- Patrulham a 6,5 unidades/s.
- Se o jogador passa a menos de 14 unidades acima de 6 unidades/s, ligam a sirene (Animator) e perseguem a 11 unidades/s por 4 s.
- Depois esperam 6 s antes de perseguir de novo.

## 5. Pistas e progressão

O mapa de cada pista nova tem 200 × 150 unidades, 6 vezes a área do circuito original (82 × 61).

| Pista | Volta | Voltas | Tempo limite | Rivais (habilidade) | Pisos e obstáculos | Música |
|---|---|---|---|---|---|---|
| Treino | 209 u | 5 | 3:00 | Nenhum | Asfalto, barris, óleo | Pure Raceway |
| Rio de Janeiro | 516 u | 3 | 4:00 | 3 (0,25) | Asfalto com muros, barris, óleo, mar | Sunny City |
| Amazônia | 500 u | 3 | 3:45 | 3 (0,45) | Terra e asfalto, grama, rochas, rio com rampa e ponte | Joyful Jungle |
| Nevada | 588 u | 3 | 4:15 | 3 (0,55) | Asfalto e terra, areia, rochas, óleo, rampa, 2 viaturas | Hot Roadway |
| Tóquio | 555 u | 3 | 4:00 | 3 (0,75) | Asfalto com muros, à noite (faróis e neon) | Neon sign Circuit |

- **Ordem de liberação:** Treino e Rio começam liberados. Terminar no pódio libera a próxima pista.
- **Calibração:** um piloto automático com o carro inicial sobra de 62 a 67 s em todas as pistas. A meta para um jogador é sobrar entre 20 e 40 s.

### Moedas

| Fonte | Valor |
|---|---|
| Colocação: 1º, 2º, 3º, 4º | 150, 100, 70 e 40, vezes 1,0 (Rio) a 1,6 (Tóquio) |
| Treino concluído | 60 |
| Moeda pega na pista | 5 cada (de 25 a 35 por pista) |
| Desafio completo (uma vez cada) | De 75 a 200 |

### Oficina

| Melhoria | Efeito por nível | Preço dos níveis 1, 2 e 3 |
|---|---|---|
| Motor | +0,6 de velocidade máxima e +0,5 de aceleração | 150, 300, 500 |
| Pneus | +2 de aderência e curvas 6% mais rápidas | 120, 250, 400 |
| Freios | +4 de força de freio | 100, 200, 350 |
| Blindagem | +1 coração | 200, 400, 650 |
| Nitro | +0,8 s de tanque | 150, 300, 450 |

Além das melhorias:
- **Pinturas:** 9, de 80 a 300 moedas. A dourada exige 6 desafios.
- **Sons de motor:** 3 (Esportivo, V8 grave, Turbo agudo).
- **Escapamentos:** 3 (Padrão, Esportivo e Turbo azul). Os melhores estouram mais e aceleram um pouco mais.

A pintura é feita por um shader próprio, que gira a cor da lataria ou tinge as partes brancas, sem precisar de sprites novos.

### Desafios

São 15 desafios, 3 por pista, de 8 tipos:
- terminar no pódio;
- terminar em 1º;
- terminar sem dano;
- terminar com no máximo N danos;
- terminar com N segundos sobrando;
- pegar N moedas;
- fazer N segundos de drift;
- usar o nitro N vezes.

## 6. Telas

| Tela | Conteúdo |
|---|---|
| Menu | Jogar, Oficina, Desafios, Instruções, Créditos, Sobre, Sair; carro da vitrine girando; moedas |
| Escolha de pista | 5 cartões com descrição, regras, recorde, desafios e bloqueio |
| Oficina | Vitrine do carro, números, 4 seletores com compra e 5 melhorias |
| Desafios | Lista completa, total e "Zerar progresso" |
| Instruções, Créditos e Sobre | Controles e dicas; fontes e licenças; disciplina, curso e integrantes |
| HUD | Tempo, volta, posição, moedas, corações, velocidade, nitro, minimapa e avisos (última volta, contramão) |
| Contagem | Semáforo com 3 luzes e número grande |
| Pausa | Continuar, Reiniciar, Voltar ao menu |
| Resultado | Título, tempo, classificação, recompensas; Tentar de novo, Próxima pista, Voltar ao menu |

O modo apresentação (F9 no menu) libera todas as pistas e dá 5000 moedas.

## 7. Arte, animação e áudio

**Arte:**
- Sprites gratuitos da CraftPix: 98 de 254, juntados em atlas comprimidos.
- As pistas novas são malhas geradas a partir da linha central, com as texturas de asfalto e de terra repetidas.
- A decoração (prédios, árvores, arbustos, rochas, arquibancadas, mar e rio) é espalhada por código, longe da pista.

**Animação com estados (Animator):**
- Luz de freio: Apagada → Acendendo → Acesa.
- Nitro: Desligado → Ligando → Ativo → Desligando.
- Sirene: Desligada ↔ Ligada.

**Outras animações:**
- Pulo na rampa (escala e sombra).
- Moedas girando, item HP+ pulsando, carro piscando depois do dano.
- Câmera que treme nas batidas.

**Efeitos:**
- Marcas de pneu.
- Fumaça de drift e poeira na terra.
- Estouros com chama no escapamento.
- Faíscas nas batidas.
- Fumaça e fogo com o carro quase quebrado.
- Fogos de artifício no pódio.
- Faróis e luzes de neon (luzes 2D) em Tóquio.

**Áudio:**
- 6 músicas, cada uma com versão de última volta. As de corrida mudam por pista.
- Motor que sobe de tom com a velocidade; o dos bots é 3D.
- Pneus cantando, nitro, estouros, batidas com volume pela força, moedas, conserto, contagem, fogos, vitória e derrota.

Os créditos completos estão no jogo (Menu > Créditos) e no [README](README.md).

## 8. Arquitetura

O `RaceManager` é o único lugar que decide o estado da corrida. Os outros scripts avisam o que aconteceu, como uma volta completa, o carro quebrado ou o tempo esgotado, ou só consultam o estado.

```
Contagem → Correndo ⇄ Pausado
               ├→ Terminou (com colocação)
               └→ Perdeu (Tempo esgotado | Carro quebrado)
```

O mesmo `CarController` move o carro do jogador e os bots. A diferença é quem chama `SetInput`:
- `PlayerDriver` lê o teclado.
- `BotDriver` segue a pista.

O progresso fica em um arquivo JSON (`SaveSystem`), e os dados de pistas, carros e peças ficam em ScriptableObjects (`GameCatalog`).

O jogo não usa variáveis `static` com estado. As cenas e os prefabs são montados por ferramentas de Editor (menu **Última Volta**), então mudar uma pista é editar uma lista de pontos.

## 9. Equipe

| Integrante | Área |
|---|---|
| Amanda Pazold dos Santos | Interface e menus |
| Cesar Otavio da Silva Boiani | Coordenação, arquitetura da corrida, progresso salvo, versionamento e build |
| Giovana Giraldeli | Arte, áudio, animações com estados e efeitos |
| Giovani Nogueira Pires | Física do carro, drift, nitro, danos, oficina e câmera |
| Marcus Vinicius da Silva Capeteruchi | Pistas, bots, polícia, itens, testes de jogo e balanceamento |

## 10. Fora do escopo

- Multijogador local ou online.
- Ranking online.
- Controle por joystick.
- Escolha de resolução e volume no jogo.
