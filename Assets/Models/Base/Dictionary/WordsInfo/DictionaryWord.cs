using UnityEngine;

[CreateAssetMenu(fileName = "NewWord", menuName = "Dictionary/Word")]
public class DictionaryWord : ScriptableObject
{
    [Header("Основное")]
    public string word;           // ПРОПИСКА

    [TextArea(3, 5)]
    public string meaning;        // Суть слова

    [Header("Из сказа")]
    public string taleTitle;      // Название сказа

    [TextArea(3, 6)]
    public string taleQuote;      // Отрывок

    [Header("Картинка")]
    public Sprite illustration;   // Иллюстрация

    [Header("Статус")]
    public bool isUnlocked = false; // Разблокировано ли слово
}
