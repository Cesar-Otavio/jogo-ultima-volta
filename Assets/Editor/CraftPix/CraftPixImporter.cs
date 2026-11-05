using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

// A licença da CraftPix não permite publicar os PNGs dos pacotes. Por isso o
// repositório público não traz esses arquivos, só os .meta deles.
// Esta ferramenta recoloca os sprites a partir dos pacotes gratuitos baixados
// do site da CraftPix:
//   - Free Racing Game Kit:          https://craftpix.net/freebies/free-racing-game-kit/
//   - Free Race Track 2D Game Tile Set: https://craftpix.net/freebies/free-race-track-2d-game-tile-set/
// Menu: Última Volta > Importar sprites da CraftPix...
public static class CraftPixImporter
{
    const string ListPath = "Assets/Editor/CraftPix/sprites_craftpix.txt";

    [MenuItem("Última Volta/Importar sprites da CraftPix...")]
    public static void Import()
    {
        string folder = EditorUtility.OpenFolderPanel("Pasta com os pacotes da CraftPix (já extraídos)", "", "");
        if (string.IsNullOrEmpty(folder))
            return;

        // Indexa todos os PNGs da pasta escolhida pelo nome do arquivo.
        Dictionary<string, List<string>> byName = new Dictionary<string, List<string>>();
        foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(file);
            if (!byName.ContainsKey(name))
                byName[name] = new List<string>();
            byName[name].Add(file);
        }

        int copied = 0, kept = 0;
        List<string> missing = new List<string>();
        foreach (string line in File.ReadAllLines(ListPath))
        {
            if (line.StartsWith("#") || line.Trim().Length == 0)
                continue;
            string[] parts = line.Split('|');
            string target = parts[0], originalName = parts[1], hash = parts[2];

            if (File.Exists(target))
            {
                kept++;
                continue;
            }

            // Vários arquivos podem ter o mesmo nome: vale o que tiver o mesmo conteúdo.
            string source = null;
            if (byName.TryGetValue(originalName, out List<string> candidates))
                foreach (string candidate in candidates)
                    if (Hash(candidate) == hash)
                        source = candidate;

            if (source == null)
            {
                missing.Add(target + " (" + originalName + ")");
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(source, target);
            copied++;
        }

        AssetDatabase.Refresh();
        string message = copied + " sprites copiados, " + kept + " já existiam.";
        if (missing.Count > 0)
            message += "\nNão encontrados (" + missing.Count + "):\n" + string.Join("\n", missing);
        Debug.Log(message);
        EditorUtility.DisplayDialog("Sprites da CraftPix", message, "OK");
    }

    static string Hash(string file)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(file))
            return System.BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
    }
}
