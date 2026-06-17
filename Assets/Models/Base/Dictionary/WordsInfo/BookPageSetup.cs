using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Повесь на Book_Opened.
/// В редакторе назначь CanvasLeft и CanvasRight.
/// Нажми ПКМ на компонент → "Setup Book Pages" чтобы создать UI.
/// </summary>
public class BookPageSetup : MonoBehaviour
{
    [Header("Canvas страниц")]
    public Canvas canvasLeft;   // картинка + отрывок
    public Canvas canvasRight;  // слово + суть + кнопки

    [Header("Размеры страницы (в метрах — совпадает с Canvas Width/Height)")]
    public float pageWidth = 0.19f;
    public float pageHeight = 0.24f;

    [Header("Шрифт (опционально)")]
    public TMP_FontAsset customFont;

    // ——— Ссылки на созданные элементы ———
    [HideInInspector] public TextMeshProUGUI wordText;
    [HideInInspector] public TextMeshProUGUI meaningText;
    [HideInInspector] public TextMeshProUGUI pageCountText;
    [HideInInspector] public Button nextButton;
    [HideInInspector] public Button prevButton;

    [HideInInspector] public Image illustrationImage;
    [HideInInspector] public TextMeshProUGUI quoteText;
    [HideInInspector] public TextMeshProUGUI taleTitleText;

    [ContextMenu("Setup Book Pages")]
    public void SetupPages()
    {
        if (canvasLeft == null || canvasRight == null)
        {
            Debug.LogError("[BookPageSetup] Назначь CanvasLeft и CanvasRight!");
            return;
        }

        SetupCanvasSize(canvasLeft);
        SetupCanvasSize(canvasRight);

        ClearCanvas(canvasLeft);
        ClearCanvas(canvasRight);

        BuildLeftPage();
        BuildRightPage();

        Debug.Log("[BookPageSetup] Страницы созданы!");
    }

    void SetupCanvasSize(Canvas canvas)
    {
        RectTransform rt = canvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(pageWidth, pageHeight);
    }

    void ClearCanvas(Canvas canvas)
    {
        foreach (Transform child in canvas.transform)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (child != null) DestroyImmediate(child.gameObject);
            };
#else
            Destroy(child.gameObject);
#endif
        }
    }

    // ——— ЛЕВАЯ СТРАНИЦА: картинка + отрывок ———
    void BuildLeftPage()
    {
        Transform root = canvasLeft.transform;

        // Фон страницы
        CreateImage(root, "Background", new Color(0.98f, 0.96f, 0.90f),
            Vector2.zero, new Vector2(pageWidth, pageHeight));

        // Картинка (верхняя часть страницы)
        illustrationImage = CreateImage(root, "Illustration", Color.gray,
            new Vector2(0, 0.055f), new Vector2(pageWidth - 0.02f, 0.11f));
        illustrationImage.preserveAspect = true;

        // Разделитель
        CreateImage(root, "Divider", new Color(0.6f, 0.4f, 0.2f),
            new Vector2(0, -0.01f), new Vector2(pageWidth - 0.03f, 0.002f));

        // Отрывок из сказа
        quoteText = CreateText(root, "QuoteText",
            "«Отрывок из сказа...»",
            new Vector2(0, -0.06f), new Vector2(pageWidth - 0.02f, 0.09f),
            0.012f, FontStyles.Italic, TextAlignmentOptions.TopLeft);

        // Название сказа
        taleTitleText = CreateText(root, "TaleTitleText",
            "— Каменный цветок",
            new Vector2(0, -0.10f), new Vector2(pageWidth - 0.02f, 0.025f),
            0.010f, FontStyles.Italic, TextAlignmentOptions.Right);
    }

    // ——— ПРАВАЯ СТРАНИЦА: слово + суть + кнопки ———
    void BuildRightPage()
    {
        Transform root = canvasRight.transform;

        // Фон
        CreateImage(root, "Background", new Color(0.98f, 0.96f, 0.90f),
            Vector2.zero, new Vector2(pageWidth, pageHeight));

        // Слово (крупно вверху)
        wordText = CreateText(root, "WordText",
            "СЛОВО",
            new Vector2(0, 0.09f), new Vector2(pageWidth - 0.01f, 0.04f),
            0.022f, FontStyles.Bold, TextAlignmentOptions.Center);
        wordText.color = new Color(0.4f, 0.1f, 0.1f);

        // Линия под словом
        CreateImage(root, "Divider", new Color(0.6f, 0.4f, 0.2f),
            new Vector2(0, 0.065f), new Vector2(pageWidth - 0.03f, 0.002f));

        // Суть/описание
        meaningText = CreateText(root, "MeaningText",
            "Описание слова...",
            new Vector2(0, 0.01f), new Vector2(pageWidth - 0.02f, 0.09f),
            0.011f, FontStyles.Normal, TextAlignmentOptions.TopLeft);

        // Разделитель
        CreateImage(root, "Divider2", new Color(0.8f, 0.7f, 0.5f),
            new Vector2(0, -0.05f), new Vector2(pageWidth - 0.05f, 0.001f));

        // Счётчик страниц
        pageCountText = CreateText(root, "PageCountText",
            "1 / 5",
            new Vector2(0, -0.085f), new Vector2(pageWidth - 0.02f, 0.02f),
            0.010f, FontStyles.Normal, TextAlignmentOptions.Center);
        pageCountText.color = Color.gray;

        // Кнопка НАЗАД ◀
        prevButton = CreateButton(root, "PrevButton", "◀",
            new Vector2(-0.05f, -0.10f), new Vector2(0.04f, 0.025f));

        // Кнопка ВПЕРЁД ▶
        nextButton = CreateButton(root, "NextButton", "▶",
            new Vector2(0.05f, -0.10f), new Vector2(0.04f, 0.025f));
    }

    // ——— Вспомогательные методы ———

    TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 anchoredPos, Vector2 size, float fontSize,
        FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = Color.black;
        tmp.enableWordWrapping = true;

        if (customFont != null) tmp.font = customFont;

        return tmp;
    }

    Image CreateImage(Transform parent, string name, Color color,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;

        return img;
    }

    Button CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.6f, 0.3f, 0.1f); // коричневый

        Button btn = go.AddComponent<Button>();

        // Текст кнопки
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);

        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 0.015f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        if (customFont != null) tmp.font = customFont;

        return btn;
    }
}
