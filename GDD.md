# Última Volta: GDD simplificado

**Grupo:** Amanda Pazold dos Santos, Cesar Otavio da Silva Boiani, Giovana Giraldeli, Giovani Nogueira Pires e Marcus Vinicius da Silva Capeteruchi
**Disciplina:** Desenvolvimento de Jogos Digitais, Ciência da Computação (P2)
**Entrega:** 19/11/2026

## 1. Visão geral

| Item | Descrição |
|---|---|
| Gênero | Corrida arcade 2D, contra o relógio |
| Vista | De cima (top-down) |
| Plataforma | PC com Windows (executável) |
| Jogadores | 1, sem adversários controlados pelo computador |
| Duração da partida | Até 3 minutos |
| Motor | Unity 6000.6.7f1 |

**Conceito:** um carro esportivo corre contra o tempo em um circuito fechado. O jogador precisa equilibrar velocidade e cuidado: ir mais rápido economiza tempo, mas cada batida forte deixa o carro mais amassado, até ele quebrar.

**Objetivo:** completar 5 voltas em até 3 minutos sem sofrer 4 danos.

## 2. Ciclo de jogo

1. No menu, o jogador lê as instruções e começa a corrida.
2. Dirige pelo circuito, desviando de barris e manchas de óleo.
3. A cada volta, confere o tempo e os corações no HUD.
4. Na última volta, a música acelera.
5. O jogo termina em vitória ou derrota, e o jogador pode tentar novamente.

## 3. Mecânicas

### Carro

| Parâmetro | Valor |
|---|---|
| Velocidade máxima | 8 unidades/s |
| Aceleração | 8 unidades/s² |
| Velocidade de ré | 3 unidades/s |
| Força de freio | 12 unidades/s² |
| Desaceleração sem acelerar | 2 unidades/s² |
| Giro máximo | 180 graus/s, atingido a partir de 2 unidades/s |

O carro só vira em movimento, e a direção inverte na ré. Ao bater, ele perde velocidade de acordo com o ângulo: de frente, para; de raspão, segue quase sem perder.

### Danos

| Situação | Efeito |
|---|---|
| Bater em um barril | Sempre causa dano |
| Bater em uma parede a 3 unidades/s ou mais, contando só a velocidade contra ela | Causa dano |
| Raspar na parede ou bater devagar | Não causa dano |
| Continuar encostado | Conta como um dano só |
| Bater de novo até 1 s depois de um dano | Não causa dano |

O limite é de 4 danos. O carro tem 5 sprites, do intacto ao quebrado, e o HUD mostra 4 corações, um a menos a cada dano.

### Voltas

A corrida tem 5 voltas. Um ponto de controle invisível, do lado oposto do circuito, faz a volta contar só depois de percorrida. Assim, a travessia logo após a largada não vale, e dar ré para cruzar a linha de novo também não.

### Óleo

Durante 1,5 s, a direção do movimento demora a acompanhar a frente do carro. Em uma curva, isso pode jogar o carro contra a parede.

## 4. Percurso

| Item | Valor |
|---|---|
| Mapa | 81,92 × 61,44 unidades |
| Volta | 209,9 unidades |
| Peças de pista | 42 (8 curvas e 34 retas) |
| Largura da pista | 5,02 unidades, contra 1,54 do carro |
| Obstáculos | 5 barris e 3 manchas de óleo |
| Câmera | Ortográfica, tamanho 7,69, limitada às bordas do mapa |

O circuito é fechado e corre no sentido horário, com curvas para os dois lados por causa de uma reentrância na parte de baixo. A largada e a chegada ficam na reta da esquerda.

Barris ficam encostados nas laterais e deixam a linha central livre. O óleo fica antes das curvas, com uma faixa livre de 1,99 unidade para desviar.

As paredes são dois laços contínuos, calculados a partir da linha central, e não uma parede por peça. Isso evita as aberturas que existiam no mapa do projeto anterior. Uma varredura automática confirmou que não há saída.

### Por que um circuito com voltas

Com 180 segundos fixos, um trajeto único e pequeno não faria o tempo pesar: a rota antiga, de 286 unidades, levava cerca de 36 s na velocidade máxima. Repetir um circuito pequeno dá a distância necessária com poucas peças.

### Calibração

| Quem | Tempo restante, com 5 voltas |
|---|---|
| Piloto automático, sem erros | 50 s |
| Primeira pessoa a testar | Entre 20 e 40 s |

A meta é sobrar de 20 a 40 segundos em uma corrida sem muitos erros.

## 5. Telas

| Tela | Conteúdo |
|---|---|
| Menu | Título, Jogar, Instruções, Sair |
| Instruções | Controles e regras |
| HUD | Cronômetro e volta no canto superior esquerdo; corações no direito |
| Pausa | Continuar, Reiniciar corrida, Voltar ao menu |
| Resultado | Vitória, Tempo esgotado ou Carro quebrado; Tentar novamente, Voltar ao menu |

## 6. Arte e áudio

**Arte:** sprites PNG dos pacotes gratuitos da CraftPix. O projeto usa 22 dos 254 sprites disponíveis. A interface usa janelas e botões só com ícone, com o texto em português.

**Áudio:** 8 arquivos com licença CC0. A música da corrida muda na última volta, o tom do motor sobe com a velocidade e o volume da batida depende da força do impacto.

Os créditos completos estão no [README](README.md).

## 7. Arquitetura

Um único gerenciador, o `RaceManager`, decide o estado da corrida. Os outros scripts avisam o que aconteceu ou consultam o estado.

```
Aguardando → Correndo ⇄ Pausado
                ├→ Vitória
                └→ Derrota (Tempo esgotado | Carro quebrado)
```

O jogo não usa variáveis `static`: os scripts se ligam por campos no Inspector. Reiniciar é carregar a cena de novo, o que restaura tempo, danos e voltas.

## 8. Fora do escopo

Adversários, várias pistas, rampa, nitro, itens de conserto, pontuação e placar. A rampa, o nitro e o item de conserto existem nos pacotes de sprites, mas não fazem parte dos requisitos.
