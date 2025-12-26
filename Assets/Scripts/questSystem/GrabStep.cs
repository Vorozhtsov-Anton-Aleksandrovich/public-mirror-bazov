using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabStep : QuestStep
{
    [SerializeField]
    private XRGrabInteractable grabObj;
    
    private void OnGrab(SelectEnterEventArgs arg0)
    {
        FinishStep();
    }

    void Awake()
    {
        grabObj.selectEntered.AddListener(OnGrab);
    }
}
