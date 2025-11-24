using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class DebugInfoStaticController
{
    public static Action OnWriteInfo;
    public static List<string> TerminalList = new List<string>();

    public static void ToTerminalQuoe(string line)
    {
        if (DebugInfoStaticController.TerminalList.Count > 24)
        {
            DebugInfoStaticController.TerminalList.RemoveAt(0);
            //Debug.Log(line);
        }
            
        TerminalList.Add(line);
    }
}
