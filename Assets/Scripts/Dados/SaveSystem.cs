using System.IO;
using UnityEngine;

// Lê e grava o progresso em um arquivo JSON na pasta de dados do jogo
// (no Windows: AppData/LocalLow/<empresa>/<produto>/progresso.json).
// Só tem funções: não guarda nenhum valor entre uma chamada e outra.
public static class SaveSystem
{
    const string FileName = "progresso.json";

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SaveData Load(GameCatalog catalog)
    {
        SaveData data = null;

        if (File.Exists(FilePath))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (System.Exception error)
            {
                Debug.LogWarning("Arquivo de progresso inválido; começando do zero. " + error.Message);
            }
        }

        if (data == null)
            data = new SaveData();

        FillDefaults(data, catalog);
        return data;
    }

    public static void Save(SaveData data)
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
    }

    public static void Delete()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }

    // Garante o mínimo para jogar: primeiras pistas liberadas e os itens
    // iniciais da oficina. Também corrige escolhas de itens que não existem.
    static void FillDefaults(SaveData data, GameCatalog catalog)
    {
        if (data.upgrades == null || data.upgrades.Length < 5)
            data.upgrades = new int[5];

        for (int i = 0; i < catalog.tracksUnlockedAtStart && i < catalog.tracks.Length; i++)
            data.Unlock(catalog.tracks[i].id);

        data.AddItem("carro", catalog.cars[0].id);
        data.AddItem("cor", catalog.colors[0].id);
        data.AddItem("motor", catalog.engines[0].id);
        data.AddItem("escape", catalog.exhausts[0].id);

        if (!data.Owns("carro", data.car)) data.car = catalog.cars[0].id;
        if (!data.Owns("cor", data.color)) data.color = catalog.colors[0].id;
        if (!data.Owns("motor", data.engine)) data.engine = catalog.engines[0].id;
        if (!data.Owns("escape", data.exhaust)) data.exhaust = catalog.exhausts[0].id;

        if (catalog.TrackById(data.selectedTrack) == null || !data.IsUnlocked(data.selectedTrack))
            data.selectedTrack = catalog.tracks[0].id;
    }
}
