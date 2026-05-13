using UnityEngine;
using System.Collections.Generic;

public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance { get; private set; }

    [Header("Все слова в игре")]
    public List<DictionaryWord> allWords = new List<DictionaryWord>();

    // Событие: вызывается когда слово разблокировано
    public System.Action<DictionaryWord> OnWordUnlocked;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Вызывай из квестовой системы:
    // DictionaryManager.Instance.UnlockWord("Прописка");
    public void UnlockWord(string wordName)
    {
        DictionaryWord found = allWords.Find(w =>
            w.word.Equals(wordName, System.StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            Debug.LogWarning($"[Dictionary] Слово '{wordName}' не найдено!");
            return;
        }

        if (found.isUnlocked)
        {
            Debug.Log($"[Dictionary] '{wordName}' уже разблокировано.");
            return;
        }

        found.isUnlocked = true;
        Debug.Log($"[Dictionary] Разблокировано слово: {wordName}");
        OnWordUnlocked?.Invoke(found);
    }

    // Возвращает только разблокированные слова
    public List<DictionaryWord> GetUnlockedWords()
    {
        return allWords.FindAll(w => w.isUnlocked);
    }
}
