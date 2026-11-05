using System.Collections.Generic;
using UnityEngine;

public enum DecorStyle { Cidade, Floresta, Deserto, CidadeNoite }
public enum ItemType { Moedas, Barril, Oleo, Conserto, Rampa, Rocha }

// Um item colocado na pista: posição como fração da volta (0 = largada,
// 0,5 = metade) e distância lateral da linha central (positivo = esquerda).
public class TrackItem
{
    public ItemType type;
    public float fraction;
    public float lateral;
    public int count = 1;

    public TrackItem(ItemType type, float fraction, float lateral = 0f, int count = 1)
    {
        this.type = type;
        this.fraction = fraction;
        this.lateral = lateral;
        this.count = count;
    }
}

// Projeto de uma pista: o traçado é uma curva suave que passa pelos pontos
// de controle, no sentido da corrida. O primeiro ponto é a largada.
public class TrackDesign
{
    public string trackId;
    public string sceneName;
    public Vector2 mapSize = new Vector2(200f, 150f);
    public Vector2[] control;
    [Tooltip("Piso de cada trecho entre dois pontos de controle. Vazio = tudo asfalto.")]
    public SurfaceType[] segmentSurface;
    public float roadScale = 1.4f;
    [Tooltip("Área de escape (grama ou areia) entre a pista e o muro.")]
    public float runoff;
    public SurfaceType offRoad = SurfaceType.Grama;
    public string groundSprite = "Fundo/Grass_Tile";
    public Color groundTint = Color.white;
    public Color background = new Color(0.1f, 0.2f, 0.12f);
    public DecorStyle style;
    public int seed = 1;
    public List<TrackItem> items = new List<TrackItem>();
    public bool sea;
    public float riverX = float.NaN;
    public float riverWidth = 9f;
    public bool police;
    public float cameraSize = 9f;
    public string[] botNames;
    public string[] botCars;
    public string[] botColors;
    public Color[] botMapColors;

    static readonly Color Blue = new Color(0.3f, 0.6f, 1f), Green = new Color(0.3f, 1f, 0.45f),
        Purple = new Color(0.8f, 0.45f, 1f), Orange = new Color(1f, 0.6f, 0.2f), Pink = new Color(1f, 0.45f, 0.75f),
        White = new Color(0.95f, 0.95f, 0.95f), Cyan = new Color(0.3f, 0.95f, 1f);

    public static List<TrackDesign> All()
    {
        return new List<TrackDesign> { Rio(), Amazonia(), Nevada(), Toquio() };
    }

    static List<TrackItem> Coins(params float[] rows)
    {
        // Cada trio: fração, lateral, quantidade.
        List<TrackItem> list = new List<TrackItem>();
        for (int i = 0; i + 2 < rows.Length; i += 3)
            list.Add(new TrackItem(ItemType.Moedas, rows[i], rows[i + 1], (int)rows[i + 2]));
        return list;
    }

    // Rio de Janeiro: orla com o mar embaixo, prédios e muros na beira da pista.
    static TrackDesign Rio()
    {
        TrackDesign d = new TrackDesign
        {
            trackId = "rio", sceneName = "Rio", style = DecorStyle.Cidade, seed = 11, sea = true,
            control = new[]
            {
                new Vector2(-40, -50), new Vector2(0, -52), new Vector2(40, -50), new Vector2(70, -42), new Vector2(80, -20),
                new Vector2(72, 0), new Vector2(80, 22), new Vector2(72, 45), new Vector2(50, 55), new Vector2(25, 48),
                new Vector2(10, 30), new Vector2(-5, 25), new Vector2(-15, 45), new Vector2(-40, 55), new Vector2(-70, 48),
                new Vector2(-82, 25), new Vector2(-70, 5), new Vector2(-82, -20), new Vector2(-72, -42),
            },
            botNames = new[] { "Bia", "Caio", "Duda" },
            botCars = new[] { "relampago", "faisca", "faisca" },
            botColors = new[] { "azul", "verde", "roxo" },
            botMapColors = new[] { Blue, Green, Purple },
        };
        d.items.AddRange(Coins(0.08f, 0f, 5, 0.22f, 1.8f, 4, 0.37f, -1.8f, 4, 0.5f, 0f, 5, 0.63f, 1.8f, 4, 0.78f, -1.8f, 4, 0.9f, 0f, 4));
        d.items.Add(new TrackItem(ItemType.Barril, 0.15f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.3f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.45f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.58f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.72f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.86f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.27f, 1.5f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.55f, -1.5f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.82f, 1.2f));
        d.items.Add(new TrackItem(ItemType.Conserto, 0.66f, 0f));
        return d;
    }

    // Amazônia: terra e asfalto, área de grama, floresta fechada e um rio
    // que corta a pista duas vezes: embaixo dá para pular pela rampa; em cima há uma ponte.
    static TrackDesign Amazonia()
    {
        SurfaceType A = SurfaceType.Asfalto, T = SurfaceType.Terra;
        TrackDesign d = new TrackDesign
        {
            trackId = "amazonia", sceneName = "Amazonia", style = DecorStyle.Floresta, seed = 23,
            runoff = 5f, offRoad = SurfaceType.Grama, groundTint = new Color(0.82f, 1f, 0.82f),
            background = new Color(0.08f, 0.18f, 0.08f), riverX = 22f, riverWidth = 9f,
            control = new[]
            {
                new Vector2(-50, -48), new Vector2(-15, -50), new Vector2(22, -50), new Vector2(50, -48), new Vector2(75, -35),
                new Vector2(80, -10), new Vector2(68, 8), new Vector2(70, 28), new Vector2(55, 50), new Vector2(22, 55),
                new Vector2(-5, 45), new Vector2(-20, 25), new Vector2(-40, 35), new Vector2(-62, 52), new Vector2(-82, 35),
                new Vector2(-78, 5), new Vector2(-80, -30), new Vector2(-72, -48),
            },
            segmentSurface = new[] { A, A, A, T, T, T, T, T, A, A, T, T, T, T, T, T, A, A },
            botNames = new[] { "Iara", "Raoni", "Tainá" },
            botCars = new[] { "faisca", "relampago", "blindado" },
            botColors = new[] { "verde", "amarelo", "original" },
            botMapColors = new[] { Green, Orange, White },
        };
        d.items.AddRange(Coins(0.1f, 0f, 5, 0.3f, 2f, 4, 0.42f, -2f, 4, 0.56f, 0f, 5, 0.7f, 2f, 4, 0.84f, -2f, 4));
        d.items.Add(new TrackItem(ItemType.Barril, 0.24f, 3f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.48f, -3f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.76f, 3f));
        d.items.Add(new TrackItem(ItemType.Rocha, 0.2f, -6f));
        d.items.Add(new TrackItem(ItemType.Rocha, 0.35f, 6f));
        d.items.Add(new TrackItem(ItemType.Rocha, 0.62f, -6f));
        d.items.Add(new TrackItem(ItemType.Rocha, 0.8f, 6f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.64f, 1.2f));
        d.items.Add(new TrackItem(ItemType.Conserto, 0.4f, 0f));
        d.items.Add(new TrackItem(ItemType.Conserto, 0.88f, 0f));
        return d;
    }

    // Nevada: deserto com retas longas de asfalto, trechos de terra, areia
    // dos lados, rochas, óleo, uma rampa e a viatura da polícia.
    static TrackDesign Nevada()
    {
        SurfaceType A = SurfaceType.Asfalto, T = SurfaceType.Terra;
        TrackDesign d = new TrackDesign
        {
            trackId = "nevada", sceneName = "Nevada", style = DecorStyle.Deserto, seed = 37,
            runoff = 6f, offRoad = SurfaceType.Areia, groundSprite = "Fundo/Soil_Tile", groundTint = new Color(1f, 0.9f, 0.7f),
            background = new Color(0.35f, 0.25f, 0.15f), police = true,
            control = new[]
            {
                new Vector2(-60, -50), new Vector2(0, -52), new Vector2(60, -50), new Vector2(82, -30), new Vector2(78, 0),
                new Vector2(55, 10), new Vector2(30, 0), new Vector2(10, 15), new Vector2(25, 35), new Vector2(55, 45),
                new Vector2(50, 62), new Vector2(0, 60), new Vector2(-50, 60), new Vector2(-80, 45),
                new Vector2(-70, 20), new Vector2(-75, -10), new Vector2(-82, -35),
            },
            segmentSurface = new[] { A, A, A, T, T, T, T, T, T, A, A, A, A, T, T, T, A },
            botNames = new[] { "Jake", "Molly", "Hank" },
            botCars = new[] { "relampago", "blindado", "faisca" },
            botColors = new[] { "preto", "original", "azul" },
            botMapColors = new[] { Orange, White, Blue },
        };
        d.items.AddRange(Coins(0.06f, 0f, 5, 0.15f, 0f, 4, 0.28f, 2f, 4, 0.4f, -2f, 4, 0.55f, 0f, 6, 0.68f, 2f, 4, 0.8f, -2f, 4, 0.92f, 0f, 4));
        d.items.Add(new TrackItem(ItemType.Rampa, 0.12f, 0f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.22f, -1.5f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.36f, 1.5f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.62f, -1.2f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.86f, 1.5f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.32f, -3f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.5f, 3f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.74f, -3f));
        for (int i = 0; i < 8; i++)
            d.items.Add(new TrackItem(ItemType.Rocha, 0.05f + i * 0.12f, (i % 2 == 0 ? 1f : -1f) * 6.5f));
        d.items.Add(new TrackItem(ItemType.Conserto, 0.47f, 0f));
        return d;
    }

    // Tóquio: noite, asfalto com muros, curvas mais fechadas e luzes de neon.
    static TrackDesign Toquio()
    {
        TrackDesign d = new TrackDesign
        {
            trackId = "toquio", sceneName = "Toquio", style = DecorStyle.CidadeNoite, seed = 53,
            groundSprite = "Fundo/Soil_Tile", groundTint = new Color(0.5f, 0.5f, 0.58f),
            background = new Color(0.03f, 0.03f, 0.08f),
            control = new[]
            {
                new Vector2(-30, -55), new Vector2(20, -55), new Vector2(55, -50), new Vector2(75, -30), new Vector2(60, -12),
                new Vector2(78, 8), new Vector2(80, 35), new Vector2(60, 55), new Vector2(30, 50), new Vector2(20, 25),
                new Vector2(0, 15), new Vector2(-15, 35), new Vector2(-35, 55), new Vector2(-65, 55), new Vector2(-80, 30),
                new Vector2(-60, 10), new Vector2(-78, -15), new Vector2(-70, -45),
            },
            botNames = new[] { "Yuki", "Kenji", "Aiko" },
            botCars = new[] { "relampago", "relampago", "faisca" },
            botColors = new[] { "rosa", "azul", "roxo" },
            botMapColors = new[] { Pink, Cyan, Purple },
        };
        d.items.AddRange(Coins(0.1f, 0f, 5, 0.25f, -1.8f, 4, 0.4f, 1.8f, 4, 0.55f, 0f, 5, 0.7f, -1.8f, 4, 0.85f, 1.8f, 4));
        d.items.Add(new TrackItem(ItemType.Barril, 0.18f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.33f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.5f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.64f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.78f, -2.9f));
        d.items.Add(new TrackItem(ItemType.Barril, 0.92f, 2.9f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.3f, 0f));
        d.items.Add(new TrackItem(ItemType.Oleo, 0.74f, 1.2f));
        d.items.Add(new TrackItem(ItemType.Conserto, 0.6f, 0f));
        return d;
    }
}
