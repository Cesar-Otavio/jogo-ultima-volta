using System.Collections.Generic;

// O que aconteceu na corrida, do ponto de vista do jogador.
public class RaceSummary
{
    public bool finished;
    public int position;
    public int carCount;
    public float raceTime;
    public float timeLeft;
    public int damage;
    public int coins;
    public float driftTime;
    public int nitroUses;
}

// O que o jogador ganhou. A tela de resultado mostra estes valores.
public class RaceResult
{
    public RaceSummary summary;
    public int positionCoins;
    public int pickupCoins;
    public int challengeCoins;
    public int TotalCoins => positionCoins + pickupCoins + challengeCoins;
    public bool newRecord;
    public TrackInfo unlockedTrack;
    // Um valor por desafio da pista: completo nesta corrida, completo antes ou não.
    public List<bool> completedNow = new List<bool>();
    public List<bool> completedBefore = new List<bool>();
}

// Regras de recompensa. Só tem funções: recebe o resumo da corrida,
// atualiza o progresso salvo e devolve o que foi ganho.
public static class RaceRewards
{
    // Quanto vale cada moeda pega na pista.
    public const int CoinValue = 5;

    public static RaceResult Apply(GameCatalog catalog, TrackInfo track, RaceSummary summary)
    {
        SaveData save = SaveSystem.Load(catalog);
        RaceResult result = new RaceResult { summary = summary };

        // As moedas pegas na pista valem mesmo sem terminar a corrida.
        result.pickupCoins = summary.coins * CoinValue;

        for (int i = 0; i < track.challenges.Length; i++)
        {
            string id = track.ChallengeId(i);
            bool before = save.HasChallenge(id);
            bool now = !before && summary.finished && IsComplete(track.challenges[i], summary);

            if (now)
            {
                save.completedChallenges.Add(id);
                result.challengeCoins += track.challenges[i].reward;
            }
            result.completedBefore.Add(before);
            result.completedNow.Add(now);
        }

        if (summary.finished)
        {
            result.positionCoins = PositionCoins(track, summary);
            result.newRecord = UpdateRecord(save, track, summary);

            // Terminar no pódio libera a próxima pista.
            TrackInfo next = catalog.NextTrack(track);
            if (summary.position <= 3 && next != null && !save.IsUnlocked(next.id))
            {
                save.Unlock(next.id);
                result.unlockedTrack = next;
            }
        }

        save.coins += result.TotalCoins;
        SaveSystem.Save(save);
        return result;
    }

    public static bool IsComplete(ChallengeInfo challenge, RaceSummary summary)
    {
        switch (challenge.type)
        {
            case ChallengeType.TerminarNoPodio: return summary.position <= 3;
            case ChallengeType.TerminarEmPrimeiro: return summary.position == 1;
            case ChallengeType.SemDanos: return summary.damage == 0;
            case ChallengeType.MaximoDeDanos: return summary.damage <= challenge.target;
            case ChallengeType.TempoSobrando: return summary.timeLeft >= challenge.target;
            case ChallengeType.ColetarMoedas: return summary.coins >= challenge.target;
            case ChallengeType.TempoDeDrift: return summary.driftTime >= challenge.target;
            case ChallengeType.UsarNitro: return summary.nitroUses >= challenge.target;
            default: return false;
        }
    }

    static int PositionCoins(TrackInfo track, RaceSummary summary)
    {
        if (summary.carCount <= 1)
            return UnityEngine.Mathf.RoundToInt(60 * track.rewardMultiplier);

        int[] coinsByPosition = { 150, 100, 70, 40 };
        int index = UnityEngine.Mathf.Clamp(summary.position - 1, 0, coinsByPosition.Length - 1);
        return UnityEngine.Mathf.RoundToInt(coinsByPosition[index] * track.rewardMultiplier);
    }

    static bool UpdateRecord(SaveData save, TrackInfo track, RaceSummary summary)
    {
        TrackRecord record = save.Record(track.id);
        if (record == null)
        {
            record = new TrackRecord { trackId = track.id };
            save.records.Add(record);
        }

        bool newRecord = record.bestTime <= 0f || summary.raceTime < record.bestTime;
        if (newRecord)
            record.bestTime = summary.raceTime;
        if (record.bestPosition == 0 || summary.position < record.bestPosition)
            record.bestPosition = summary.position;
        return newRecord;
    }
}
