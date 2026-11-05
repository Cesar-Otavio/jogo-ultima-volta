using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Uma linha da oficina com setas para trocar a opção (modelo, cor, motor, escapamento).
[System.Serializable]
public class GarageRow
{
    public TMP_Text value;
    public TMP_Text description;
    public Button actionButton;
    public TMP_Text actionLabel;
}

// Uma linha de melhoria, com o nível atual e o botão de comprar o próximo.
[System.Serializable]
public class UpgradeRow
{
    public TMP_Text level;
    public Button buyButton;
    public TMP_Text buyLabel;
}

// Oficina: escolhe modelo, pintura, som do motor e escapamento, e compra
// melhorias com as moedas ganhas nas corridas.
public class GarageScreen : MonoBehaviour
{
    public const int Model = 0, Paint = 1, Engine = 2, Exhaust = 3;

    public MenuController menu;
    public GarageRow[] rows = new GarageRow[4];
    public UpgradeRow[] upgradeRows = new UpgradeRow[5];
    public TMP_Text statsText;

    [Header("Prévia")]
    public SpriteRenderer previewCar;
    public AudioSource enginePreview;
    public AudioLowPassFilter enginePreviewFilter;

    // Opção que está sendo vista em cada linha (pode ainda não ser do jogador).
    private readonly int[] browsing = new int[4];

    public void Refresh()
    {
        GameCatalog catalog = menu.catalog;
        SaveData data = menu.Data;
        browsing[Model] = IndexOf(catalog.cars, data.car);
        browsing[Paint] = IndexOf(catalog.colors, data.color);
        browsing[Engine] = IndexOf(catalog.engines, data.engine);
        browsing[Exhaust] = IndexOf(catalog.exhausts, data.exhaust);
        UpdateView();
    }

    public void Previous(int row) => Browse(row, -1);
    public void Next(int row) => Browse(row, 1);

    void Browse(int row, int step)
    {
        int count = Count(row);
        browsing[row] = (browsing[row] + step + count) % count;
        menu.PlayClick();
        UpdateView();
    }

    // Botão da linha: comprar, usar ou nada (já em uso / bloqueado).
    public void Action(int row)
    {
        SaveData data = menu.Data;
        string kind = Kind(row);
        string id = Id(row);

        if (data.Owns(kind, id))
        {
            Select(row, id);
            menu.PlayClick();
        }
        else if (menu.Data.completedChallenges.Count < Required(row))
        {
            menu.PlayError();
            menu.ShowToast("Complete " + Required(row) + " desafios para liberar este item");
        }
        else if (data.coins < Price(row))
        {
            menu.PlayError();
            menu.ShowToast("Moedas insuficientes: faltam " + (Price(row) - data.coins));
        }
        else
        {
            data.coins -= Price(row);
            data.AddItem(kind, id);
            Select(row, id);
            menu.PlayUnlock();
            menu.ShowToast("Comprado!");
        }

        menu.SaveProgress();
        UpdateView();
    }

    public void BuyUpgrade(int type)
    {
        SaveData data = menu.Data;
        UpgradeInfo upgrade = menu.catalog.Upgrade((UpgradeType)type);
        int level = data.UpgradeLevel((UpgradeType)type);

        if (level >= GameCatalog.MaxUpgradeLevel)
            return;

        int price = upgrade.prices[level];
        if (data.coins < price)
        {
            menu.PlayError();
            menu.ShowToast("Moedas insuficientes: faltam " + (price - data.coins));
            return;
        }

        data.coins -= price;
        data.upgrades[type] = level + 1;
        menu.SaveProgress();
        menu.PlayUnlock();
        menu.ShowToast(upgrade.displayName + " no nível " + (level + 1) + "!");
        UpdateView();
    }

    // Toca o motor escolhido subindo de giro, como numa acelerada.
    public void ListenEngine()
    {
        StopAllCoroutines();
        StartCoroutine(Rev(menu.catalog.engines[browsing[Engine]]));
    }

    IEnumerator Rev(EngineOption engine)
    {
        enginePreviewFilter.cutoffFrequency = engine.lowPassCutoff;
        enginePreview.volume = 0.6f;
        enginePreview.Play();
        for (float t = 0f; t < 1.6f; t += Time.deltaTime)
        {
            float rev = t < 1.1f ? t / 1.1f : 1f - (t - 1.1f) / 0.5f;
            enginePreview.pitch = Mathf.Lerp(engine.minPitch, engine.maxPitch, rev);
            yield return null;
        }
        enginePreview.Stop();
    }

    void Select(int row, string id)
    {
        SaveData data = menu.Data;
        if (row == Model) data.car = id;
        else if (row == Paint) data.color = id;
        else if (row == Engine) data.engine = id;
        else data.exhaust = id;
    }

    void UpdateView()
    {
        GameCatalog catalog = menu.catalog;
        SaveData data = menu.Data;

        for (int row = 0; row < rows.Length; row++)
        {
            GarageRow view = rows[row];
            string kind = Kind(row);
            string id = Id(row);
            bool owned = data.Owns(kind, id);
            bool selected = id == SelectedId(row);

            view.value.text = Name(row);
            view.description.text = Description(row);
            if (selected)
                view.actionLabel.text = "Em uso";
            else if (owned)
                view.actionLabel.text = "Usar";
            else if (data.completedChallenges.Count < Required(row))
                view.actionLabel.text = "Bloqueado (" + Required(row) + " desafios)";
            else
                view.actionLabel.text = "Comprar: " + Price(row);
            view.actionButton.interactable = !selected;
        }

        for (int type = 0; type < upgradeRows.Length; type++)
        {
            UpgradeRow view = upgradeRows[type];
            UpgradeInfo upgrade = catalog.Upgrade((UpgradeType)type);
            int level = data.UpgradeLevel((UpgradeType)type);
            view.level.text = upgrade.displayName + "  " + Dots(level) + "\n<size=70%>" + upgrade.description + "</size>";
            bool maxed = level >= GameCatalog.MaxUpgradeLevel;
            view.buyLabel.text = maxed ? "Máximo" : "Melhorar: " + upgrade.prices[level];
            view.buyButton.interactable = !maxed;
        }

        // A prévia mostra o modelo e a cor que estão sendo vistos.
        CarModel model = catalog.cars[browsing[Model]];
        previewCar.sprite = model.damageSprites[0];
        previewCar.transform.localScale = Vector3.one * model.scale * 2.4f;
        CarSetup.ApplyPaint(previewCar, model, catalog.colors[browsing[Paint]]);

        statsText.text = StatsFor(model);
    }

    // Números do modelo visto, já com as melhorias compradas.
    string StatsFor(CarModel model)
    {
        CarStats stats = model.stats.Copy();
        GameCatalog.ApplyUpgrades(stats, menu.Data);
        stats.acceleration += menu.catalog.exhausts[browsing[Exhaust]].accelerationBonus;
        return "Velocidade máxima: " + Mathf.RoundToInt(stats.maxSpeed * 18f) + " km/h\n"
               + "Aceleração: " + stats.acceleration.ToString("0.0") + "\n"
               + "Curvas: " + stats.turnSpeed.ToString("0") + " °/s\n"
               + "Freios: " + stats.brakeForce.ToString("0") + "\n"
               + "Corações: " + stats.hearts + "\n"
               + "Nitro: " + stats.nitroCapacity.ToString("0.0") + " s";
    }

    static string Dots(int level)
    {
        string text = "";
        for (int i = 0; i < GameCatalog.MaxUpgradeLevel; i++)
            text += i < level ? "<color=#FFD95A>■</color>" : "<color=#666666>■</color>";
        return text;
    }

    // ---------- Dados de cada linha ----------

    int Count(int row)
    {
        GameCatalog c = menu.catalog;
        return row == Model ? c.cars.Length : row == Paint ? c.colors.Length : row == Engine ? c.engines.Length : c.exhausts.Length;
    }

    string Kind(int row) => row == Model ? "carro" : row == Paint ? "cor" : row == Engine ? "motor" : "escape";

    string Id(int row)
    {
        GameCatalog c = menu.catalog;
        int i = browsing[row];
        return row == Model ? c.cars[i].id : row == Paint ? c.colors[i].id : row == Engine ? c.engines[i].id : c.exhausts[i].id;
    }

    string SelectedId(int row)
    {
        SaveData d = menu.Data;
        return row == Model ? d.car : row == Paint ? d.color : row == Engine ? d.engine : d.exhaust;
    }

    string Name(int row)
    {
        GameCatalog c = menu.catalog;
        int i = browsing[row];
        return row == Model ? c.cars[i].displayName : row == Paint ? c.colors[i].displayName
            : row == Engine ? c.engines[i].displayName : c.exhausts[i].displayName;
    }

    string Description(int row)
    {
        GameCatalog c = menu.catalog;
        int i = browsing[row];
        return row == Model ? c.cars[i].description : row == Paint ? "Pintura do carro"
            : row == Engine ? c.engines[i].description : c.exhausts[i].description;
    }

    int Price(int row)
    {
        GameCatalog c = menu.catalog;
        int i = browsing[row];
        return row == Model ? c.cars[i].price : row == Paint ? c.colors[i].price : row == Engine ? c.engines[i].price : c.exhausts[i].price;
    }

    int Required(int row)
    {
        GameCatalog c = menu.catalog;
        int i = browsing[row];
        return row == Model ? 0 : row == Paint ? c.colors[i].challengesRequired
            : row == Engine ? c.engines[i].challengesRequired : c.exhausts[i].challengesRequired;
    }

    static int IndexOf(CarModel[] list, string id) { for (int i = 0; i < list.Length; i++) if (list[i].id == id) return i; return 0; }
    static int IndexOf(ColorOption[] list, string id) { for (int i = 0; i < list.Length; i++) if (list[i].id == id) return i; return 0; }
    static int IndexOf(EngineOption[] list, string id) { for (int i = 0; i < list.Length; i++) if (list[i].id == id) return i; return 0; }
    static int IndexOf(ExhaustOption[] list, string id) { for (int i = 0; i < list.Length; i++) if (list[i].id == id) return i; return 0; }
}
