using UnityEngine;

// Números que definem como o carro anda. A oficina soma as melhorias a estes valores.
[System.Serializable]
public class CarStats
{
    public float maxSpeed = 10f;
    public float acceleration = 8.5f;
    public float brakeForce = 14f;
    public float turnSpeed = 175f;
    public float grip = 10f;
    public float nitroCapacity = 2.5f;
    public int hearts = 4;
    public float mass = 1f;

    public CarStats Copy()
    {
        return (CarStats)MemberwiseClone();
    }
}

// Um modelo de carro da oficina, com os 5 sprites de dano.
[CreateAssetMenu(menuName = "Última Volta/Modelo de carro")]
public class CarModel : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public int price;

    [Tooltip("Do carro intacto ao quebrado.")]
    public Sprite[] damageSprites;
    [Tooltip("Escala do sprite na cena.")]
    public float scale = 0.31f;
    [Tooltip("Matiz da pintura original, de 0 a 360. Usado para trocar a cor.")]
    public float baseHue;
    [Tooltip("Ligado quando a pintura original é branca: a cor é aplicada como tinta.")]
    public bool whiteBody;

    public CarStats stats = new CarStats();
}
