using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Вешается на Book_Opened.
/// Управляет листанием страниц и передаёт данные в BookPageUI.
/// </summary>
public class BookPageManager : MonoBehaviour
{
    [Header("UI страницы")]
    public BookPageUI pageUI; // ссылка на компонент Canvas

    [Header("Листание (опционально)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable nextPageButton;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable prevPageButton;

    private List<DictionaryWord> unlockedWords = new List<DictionaryWord>();
    private int currentIndex = 0;

    void OnEnable()
    {
        // Подписываемся на разблокировку новых слов
        if (DictionaryManager.Instance != null)
            DictionaryManager.Instance.OnWordUnlocked += OnWordUnlocked;

        // Подписываемся на кнопки листания
        if (nextPageButton != null)
            nextPageButton.selectEntered.AddListener(_ => NextPage());
        if (prevPageButton != null)
            prevPageButton.selectEntered.AddListener(_ => PrevPage());

        RefreshWords();
    }

    void OnDisable()
    {
        if (DictionaryManager.Instance != null)
            DictionaryManager.Instance.OnWordUnlocked -= OnWordUnlocked;

        if (nextPageButton != null)
            nextPageButton.selectEntered.RemoveAllListeners();
        if (prevPageButton != null)
            prevPageButton.selectEntered.RemoveAllListeners();
    }

    // Обновляем список разблокированных слов
    void RefreshWords()
    {
        if (DictionaryManager.Instance == null) return;

        unlockedWords = DictionaryManager.Instance.GetUnlockedWords();

        if (unlockedWords.Count > 0)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, unlockedWords.Count - 1);
            ShowCurrentPage();
        }
        else
        {
            pageUI?.ShowEmpty();
        }
    }

    // Новое слово разблокировано — обновляем и показываем его
    void OnWordUnlocked(DictionaryWord word)
    {
        RefreshWords();
        // Перелистываем на только что добавленное слово
        currentIndex = unlockedWords.IndexOf(word);
        ShowCurrentPage();
    }

    void ShowCurrentPage()
    {
        if (unlockedWords.Count == 0) return;
        pageUI?.ShowWord(unlockedWords[currentIndex], currentIndex + 1, unlockedWords.Count);
    }

    public void NextPage()
    {
        if (currentIndex < unlockedWords.Count - 1)
        {
            currentIndex++;
            ShowCurrentPage();
        }
    }

    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowCurrentPage();
        }
    }

    void Start()
    {
        RefreshWords();
    }
}
