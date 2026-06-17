using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Вешается на Canvas (дочерний объект Book_Opened).
/// Заполняет страницу данными из DictionaryWord.
/// </summary>
public class BookPageUI : MonoBehaviour
{
    [Header("Текстовые поля (TextMeshPro)")]
    public TextMeshProUGUI wordText;        // Большое слово вверху
    public TextMeshProUGUI meaningText;     // Суть/описание
    public TextMeshProUGUI taleQuoteText;   // Отрывок из сказа
    public TextMeshProUGUI taleTitleText;   // Название сказа
    public TextMeshProUGUI pageCountText;   // "1 / 3"

    [Header("Картинка")]
    public Image illustrationImage;         // Иллюстрация
    public GameObject illustrationFrame;    // Рамка (скрываем если нет картинки)

    [Header("Пустая страница")]
    public GameObject emptyPagePanel;       // Показываем если слов нет
    public GameObject contentPanel;         // Основной контент

    public void ShowWord(DictionaryWord data, int current, int total)
    {
        if (emptyPagePanel != null) emptyPagePanel.SetActive(false);
        if (contentPanel != null) contentPanel.SetActive(true);

        if (wordText != null) wordText.text = data.word.ToUpper();
        if (meaningText != null) meaningText.text = data.meaning;
        if (taleQuoteText != null) taleQuoteText.text = $"\"{data.taleQuote}\"";
        if (taleTitleText != null) taleTitleText.text = $"— {data.taleTitle}";
        if (pageCountText != null) pageCountText.text = $"{current} / {total}";

        // Картинка
        if (illustrationImage != null)
        {
            bool hasImage = data.illustration != null;
            illustrationImage.sprite = data.illustration;
            if (illustrationFrame != null) illustrationFrame.SetActive(hasImage);
            illustrationImage.gameObject.SetActive(hasImage);
        }
    }

    public void ShowEmpty()
    {
        if (emptyPagePanel != null) emptyPagePanel.SetActive(false);
        if (contentPanel != null) contentPanel.SetActive(true);
    }
}
