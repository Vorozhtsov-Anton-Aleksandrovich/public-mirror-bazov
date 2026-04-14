using TMPro;
using UnityEngine;

public class DebugInfoWriter : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro console; // лучше сделать private, если не нужно извне

    private void Start()
    {
        // Если ссылка не назначена в инспекторе – попытаемся найти компонент на этом же объекте
        if (console == null)
            console = GetComponent<TextMeshPro>();

        // Если всё равно null – выведем предупреждение и не подписываемся
        if (console == null)
        {
            Debug.LogError("DebugInfoWriter: TextMeshPro component not assigned and not found on GameObject!");
            return;
        }

        DebugInfoStaticController.OnWriteInfo += UpdateTerminal;
        DebugInfoStaticController.ToTerminalQuoe("terminal is INIT");
    }

    private void UpdateTerminal()
    {
        if (console == null) return; // защита от null

        string allMessages = "";
        foreach (string line in DebugInfoStaticController.TerminalList)
        {
            allMessages += line + "\n";
        }

        console.text = allMessages;
    }

    // Вместо FixedUpdate можно вызывать обновление только при событии,
    // но если нужно гарантированно обновлять каждый кадр – оставьте.
    // private void FixedUpdate() { UpdateTerminal(); }

    private void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать утечек памяти
        DebugInfoStaticController.OnWriteInfo -= UpdateTerminal;
    }
}