using UnityEngine;

public enum ChallengeType
{
    TerminarNoPodio,
    TerminarEmPrimeiro,
    SemDanos,
    MaximoDeDanos,
    TempoSobrando,
    ColetarMoedas,
    TempoDeDrift,
    UsarNitro
}

// Um desafio da pista. Só vale se o jogador cruzar a chegada.
[System.Serializable]
public class ChallengeInfo
{
    public ChallengeType type;
    [Tooltip("Valor do desafio: segundos, moedas, danos ou usos, conforme o tipo.")]
    public float target;
    [Tooltip("Moedas ganhas ao completar o desafio pela primeira vez.")]
    public int reward = 100;

    public string Description()
    {
        int value = Mathf.RoundToInt(target);
        switch (type)
        {
            case ChallengeType.TerminarNoPodio: return "Termine no pódio (até 3º lugar)";
            case ChallengeType.TerminarEmPrimeiro: return "Termine em 1º lugar";
            case ChallengeType.SemDanos: return "Termine sem nenhum dano";
            case ChallengeType.MaximoDeDanos: return "Termine com no máximo " + value + (value == 1 ? " dano" : " danos");
            case ChallengeType.TempoSobrando: return "Termine com " + value + " s sobrando";
            case ChallengeType.ColetarMoedas: return "Pegue " + value + " moedas na pista";
            case ChallengeType.TempoDeDrift: return "Faça " + value + " s de drift na corrida";
            case ChallengeType.UsarNitro: return "Use o nitro " + value + " vezes";
            default: return "";
        }
    }
}

// Dados de uma pista: cena, regras, música e desafios.
// Cada pista é um arquivo em Assets/Dados/Pistas.
[CreateAssetMenu(menuName = "Última Volta/Pista")]
public class TrackInfo : ScriptableObject
{
    [Tooltip("Identificador usado no arquivo de progresso. Não mude depois de criado.")]
    public string id;
    public string displayName;
    public string country;
    [TextArea] public string description;
    public string sceneName;

    [Header("Regras")]
    public int laps = 3;
    [Tooltip("Tempo limite da corrida, em segundos.")]
    public float timeLimit = 180f;
    [Tooltip("Quantidade de adversários controlados pelo computador.")]
    public int botCount = 3;
    [Tooltip("Habilidade dos bots, de 0 (fácil) a 1 (difícil).")]
    [Range(0f, 1f)] public float botSkill = 0.5f;
    [Tooltip("Multiplica as moedas ganhas pela colocação.")]
    public float rewardMultiplier = 1f;

    [Header("Ambiente")]
    public bool isNight;
    public Color themeColor = Color.white;

    [Header("Música")]
    public AudioClip music;
    public AudioClip lastLapMusic;

    [Header("Desafios")]
    public ChallengeInfo[] challenges;

    public string ChallengeId(int index)
    {
        return id + "_" + index;
    }
}
