using System.Collections.Generic;
using UnityEditor;

// Mantém a lista de cenas do build: o menu primeiro, depois as pistas.
public static class SceneList
{
    public static readonly string[] Scenes =
    {
        "Assets/Scenes/Menu.unity",
        "Assets/Scenes/Treino.unity",
        "Assets/Scenes/Pistas/Rio.unity",
        "Assets/Scenes/Pistas/Amazonia.unity",
        "Assets/Scenes/Pistas/Nevada.unity",
        "Assets/Scenes/Pistas/Toquio.unity",
    };

    public static void Update()
    {
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();
        foreach (string scene in Scenes)
            if (System.IO.File.Exists(scene))
                list.Add(new EditorBuildSettingsScene(scene, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
