using UnityEngine;

public enum UpgradeType { Motor, Pneus, Freios, Blindagem, Nitro }

[System.Serializable]
public class ColorOption
{
    public string id;
    public string displayName;
    public Color color = Color.white;
    [Tooltip("Mantém a pintura original do modelo.")]
    public bool original;
    public int price;
    [Tooltip("Desafios completos necessários para liberar a compra.")]
    public int challengesRequired;
}

[System.Serializable]
public class EngineOption
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    [Tooltip("Tom do motor parado e na velocidade máxima.")]
    public float minPitch = 0.8f;
    public float maxPitch = 1.8f;
    [Tooltip("Corta os agudos acima desta frequência (Hz). 22000 = som original.")]
    public float lowPassCutoff = 22000f;
    public int price;
    public int challengesRequired;
}

[System.Serializable]
public class ExhaustOption
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    [Tooltip("Chance de estouro ao tirar o pé do acelerador em alta velocidade.")]
    [Range(0f, 1f)] public float popChance = 0.25f;
    public Color flameColor = new Color(1f, 0.6f, 0.2f);
    [Tooltip("Aceleração extra, em unidades/s².")]
    public float accelerationBonus;
    public int price;
    public int challengesRequired;
}

[System.Serializable]
public class UpgradeInfo
{
    public UpgradeType type;
    public string displayName;
    [TextArea] public string description;
    [Tooltip("Preço de cada nível, do 1 ao 3.")]
    public int[] prices = { 150, 300, 500 };
}

// Lista de tudo que existe no jogo: pistas, carros e peças da oficina.
// Fica em Assets/Dados/Catalogo.asset e é usada pelo menu e pelas corridas.
[CreateAssetMenu(menuName = "Última Volta/Catálogo")]
public class GameCatalog : ScriptableObject
{
    public const int MaxUpgradeLevel = 3;

    public TrackInfo[] tracks;
    public CarModel[] cars;
    public ColorOption[] colors;
    public EngineOption[] engines;
    public ExhaustOption[] exhausts;
    public UpgradeInfo[] upgrades;

    [Tooltip("Pistas liberadas no primeiro jogo, contadas a partir da primeira.")]
    public int tracksUnlockedAtStart = 2;

    public TrackInfo TrackById(string id)
    {
        foreach (TrackInfo track in tracks)
            if (track.id == id)
                return track;
        return null;
    }

    public TrackInfo NextTrack(TrackInfo track)
    {
        int index = System.Array.IndexOf(tracks, track);
        return index >= 0 && index + 1 < tracks.Length ? tracks[index + 1] : null;
    }

    public CarModel CarById(string id)
    {
        foreach (CarModel car in cars)
            if (car.id == id)
                return car;
        return cars[0];
    }

    public ColorOption ColorById(string id)
    {
        foreach (ColorOption option in colors)
            if (option.id == id)
                return option;
        return colors[0];
    }

    public EngineOption EngineById(string id)
    {
        foreach (EngineOption option in engines)
            if (option.id == id)
                return option;
        return engines[0];
    }

    public ExhaustOption ExhaustById(string id)
    {
        foreach (ExhaustOption option in exhausts)
            if (option.id == id)
                return option;
        return exhausts[0];
    }

    public UpgradeInfo Upgrade(UpgradeType type)
    {
        foreach (UpgradeInfo upgrade in upgrades)
            if (upgrade.type == type)
                return upgrade;
        return null;
    }

    public int TotalChallenges()
    {
        int total = 0;
        foreach (TrackInfo track in tracks)
            total += track.challenges.Length;
        return total;
    }

    // Números do carro do jogador: modelo escolhido + melhorias + escapamento.
    public CarStats StatsFor(SaveData save)
    {
        CarStats stats = CarById(save.car).stats.Copy();
        ApplyUpgrades(stats, save);
        stats.acceleration += ExhaustById(save.exhaust).accelerationBonus;
        return stats;
    }

    // Cada nível de melhoria soma um valor fixo. Estes números aparecem
    // na oficina, por isso ficam em um lugar só.
    public static void ApplyUpgrades(CarStats stats, SaveData save)
    {
        int motor = save.UpgradeLevel(UpgradeType.Motor);
        int tires = save.UpgradeLevel(UpgradeType.Pneus);
        int brakes = save.UpgradeLevel(UpgradeType.Freios);
        int armor = save.UpgradeLevel(UpgradeType.Blindagem);
        int nitro = save.UpgradeLevel(UpgradeType.Nitro);

        stats.maxSpeed += 0.6f * motor;
        stats.acceleration += 0.5f * motor;
        stats.grip += 2f * tires;
        stats.turnSpeed *= 1f + 0.06f * tires;
        stats.brakeForce += 4f * brakes;
        stats.hearts += armor;
        stats.nitroCapacity += 0.8f * nitro;
    }
}
