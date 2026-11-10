using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Gera o executável do jogo para Windows, com nome, versão e ícone.
// Menu: Última Volta > 6. Gerar executável
// Também roda pela linha de comando:
//   Unity.exe -batchmode -quit -projectPath <pasta> -executeMethod BuildScript.RebuildEverything
public static class BuildScript
{
    public const string ExePath = "Builds/UltimaVolta/UltimaVolta.exe";
    public const string Version = "2.0";

    [MenuItem("Última Volta/6. Gerar executável")]
    public static void BuildGame()
    {
        ApplyProjectInfo();

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = System.Array.ConvertAll(EditorBuildSettings.scenes, scene => scene.path),
            locationPathName = ExePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        });

        BuildSummary summary = report.summary;
        Debug.Log("Build: " + summary.result + ", " + (summary.totalSize / 1048576f).ToString("0.0")
                  + " MB, " + summary.totalErrors + " erros.");
        if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // Monta tudo do zero (assets, prefabs, pistas e menu) e gera o executável.
    [MenuItem("Última Volta/Reconstruir tudo e gerar executável")]
    public static void RebuildEverything()
    {
        AssetSetup.PrepareAll();
        CarPrefabBuilder.BuildAll();
        TrainingConverter.Convert();
        TrackBuilder.BuildAll();
        MenuBuilder.Build();
        BuildGame();
    }

    // Nome da empresa e do jogo, versão e ícone do executável.
    public static void ApplyProjectInfo()
    {
        PlayerSettings.companyName = "Grupo Ultima Volta";
        PlayerSettings.productName = "Última Volta";
        PlayerSettings.bundleVersion = Version;

        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetSetup.GeneratedFolder + "/Icone.png");
        if (icon != null)
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
    }
}
