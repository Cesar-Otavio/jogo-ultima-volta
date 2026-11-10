using System.Collections.Generic;

[System.Serializable]
public class TrackRecord
{
    public string trackId;
    public float bestTime;
    public int bestPosition;
    [UnityEngine.Tooltip("Ranking da pista: os 5 melhores tempos, do menor para o maior.")]
    public List<float> topTimes = new List<float>();
}

// Tudo o que o jogo guarda entre uma partida e outra.
// O SaveSystem grava esta classe em um arquivo JSON.
[System.Serializable]
public class SaveData
{
    public int coins;
    public List<string> unlockedTracks = new List<string>();
    public List<string> completedChallenges = new List<string>();
    public List<string> ownedItems = new List<string>();
    public List<TrackRecord> records = new List<TrackRecord>();

    [UnityEngine.Header("Escolhas atuais")]
    public string selectedTrack = "";
    public string car = "";
    public string color = "";
    public string engine = "";
    public string exhaust = "";
    public int[] upgrades = new int[5];

    // Itens da oficina são guardados como "tipo:id", por exemplo "cor:azul".
    public bool Owns(string kind, string id)
    {
        return ownedItems.Contains(kind + ":" + id);
    }

    public void AddItem(string kind, string id)
    {
        if (!Owns(kind, id))
            ownedItems.Add(kind + ":" + id);
    }

    public bool IsUnlocked(string trackId)
    {
        return unlockedTracks.Contains(trackId);
    }

    public void Unlock(string trackId)
    {
        if (!IsUnlocked(trackId))
            unlockedTracks.Add(trackId);
    }

    public bool HasChallenge(string challengeId)
    {
        return completedChallenges.Contains(challengeId);
    }

    public int UpgradeLevel(UpgradeType type)
    {
        int index = (int)type;
        return upgrades != null && index < upgrades.Length ? upgrades[index] : 0;
    }

    public TrackRecord Record(string trackId)
    {
        foreach (TrackRecord record in records)
            if (record.trackId == trackId)
                return record;
        return null;
    }
}
