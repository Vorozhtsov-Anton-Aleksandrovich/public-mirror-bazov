using TMPro;
using UnityEngine;

public class DebugInfoWriter : MonoBehaviour
{
    [SerializeField]
    public TextMeshPro console;

    void Start()
    {
        DebugInfoStaticController.OnWriteInfo += updateTerminal;
        DebugInfoStaticController.ToTerminalQuoe("terminal is INIT");
    }

    private void updateTerminal()
    {
        string allMaseges = "";
        foreach (string line in DebugInfoStaticController.TerminalList)
        {
            allMaseges += line + "\n";
        }

        console.text = allMaseges;
    }

    void FixedUpdate()
    {
        updateTerminal();
    }

    ~DebugInfoWriter()
    {
        DebugInfoStaticController.OnWriteInfo -= updateTerminal;
    }
}
