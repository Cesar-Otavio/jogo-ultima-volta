using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;

// Funções curtas para montar telas por código, sempre com o mesmo visual:
// janela e faixas da CraftPix, fonte LiberationSans e textos em português.
// Resolução de referência: 1920 x 1080.
public static class UIBuilder
{
    public static readonly Color TextColor = new Color(1f, 0.97f, 0.9f);
    public static readonly Color Gold = new Color(1f, 0.85f, 0.35f);

    public static TMP_FontAsset Font =>
        AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

    public static Sprite UISprite(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/" + name + ".png");

    public static Sprite Generated(string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Gerados/" + name + ".png");

    public static Canvas Canvas(string name, int sortingOrder)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static void EventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // Cria um objeto de interface preso a um ponto do pai (anchor).
    // anchor (0,0) = canto inferior esquerdo; (0.5,0.5) = centro; (1,1) = canto superior direito.
    public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    // Ocupa o pai inteiro, com uma margem opcional.
    public static RectTransform Fill(Transform parent, string name, float margin = 0f)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(margin, margin);
        rect.offsetMax = new Vector2(-margin, -margin);
        return rect;
    }

    public static UImage Image(Transform parent, string name, Sprite sprite, Color color,
        Vector2 anchor, Vector2 position, Vector2 size, bool sliced = false)
    {
        RectTransform rect = Rect(parent, name, anchor, position, size);
        UImage image = rect.gameObject.AddComponent<UImage>();
        image.sprite = sprite;
        image.color = color;
        image.type = sliced ? UImage.Type.Sliced : UImage.Type.Simple;
        image.preserveAspect = !sliced && sprite != null;
        image.raycastTarget = false;
        return image;
    }

    // Fundo escuro que cobre a tela inteira (pausa, resultado, menus).
    public static UImage Overlay(Transform parent, string name, float alpha)
    {
        RectTransform rect = Fill(parent, name);
        UImage image = rect.gameObject.AddComponent<UImage>();
        image.sprite = Generated("Pixel");
        image.color = new Color(0f, 0f, 0f, alpha);
        return image;
    }

    public static TMP_Text Text(Transform parent, string name, string text, float size,
        Vector2 anchor, Vector2 position, Vector2 rectSize, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        RectTransform rect = Rect(parent, name, anchor, position, rectSize);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = Font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = TextColor;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    // Botão com a faixa da CraftPix (esticada em 9 partes) e texto em português.
    public static Button Button(Transform parent, string name, string label, Vector2 anchor,
        Vector2 position, Vector2 size, UnityAction action, float fontSize = 36f)
    {
        UImage image = Image(parent, name, UISprite("Faixa"), Color.white, anchor, position, size, true);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.92f, 0.7f);
        colors.pressedColor = new Color(0.8f, 0.75f, 0.6f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        button.colors = colors;

        TMP_Text text = Text(image.transform, "Rotulo", label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        if (action != null)
            UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    public static void AddCall(Button button, UnityAction<int> action, int argument)
    {
        UnityEventTools.AddIntPersistentListener(button.onClick, action, argument);
    }

    // Janela da CraftPix com título.
    public static RectTransform Window(Transform parent, string name, string title, Vector2 size, out TMP_Text titleText)
    {
        UImage window = Image(parent, name, UISprite("Janela"), Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, size, true);
        titleText = Text(window.transform, "Titulo", title, 56f, new Vector2(0.5f, 1f), new Vector2(0f, -40f),
            new Vector2(size.x - 160f, 90f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Gold;
        return window.rectTransform;
    }
}
