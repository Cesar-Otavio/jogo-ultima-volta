// Tipos de piso. Cada um muda a velocidade máxima e a aderência do carro.
public enum SurfaceType { Asfalto, Terra, Grama, Areia, Agua }

public static class SurfaceInfo
{
    // Fração da velocidade máxima que o carro alcança neste piso.
    public static float SpeedFactor(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Terra: return 0.92f;
            case SurfaceType.Grama: return 0.6f;
            case SurfaceType.Areia: return 0.65f;
            case SurfaceType.Agua: return 0.45f;
            default: return 1f;
        }
    }

    // Fração da aderência normal. Menos aderência = o carro escorrega mais.
    public static float GripFactor(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Terra: return 0.45f;
            case SurfaceType.Grama: return 0.6f;
            case SurfaceType.Areia: return 0.5f;
            case SurfaceType.Agua: return 0.4f;
            default: return 1f;
        }
    }

    // Pisos soltos levantam poeira quando o carro passa rápido.
    public static bool MakesDust(SurfaceType surface)
    {
        return surface == SurfaceType.Terra || surface == SurfaceType.Areia;
    }

    public static string Name(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Terra: return "Terra";
            case SurfaceType.Grama: return "Grama";
            case SurfaceType.Areia: return "Areia";
            case SurfaceType.Agua: return "Água";
            default: return "Asfalto";
        }
    }
}
