using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class PinchGrabInteractor : XRBaseInteractor
{
    public bool isGrabbing => hasSelection;

    public void StartGrab(IXRSelectInteractable interactable)
    {
        if (hasSelection || !CanSelect(interactable)) return;
        interactionManager?.SelectEnter(this, interactable);
    }

    public void StopGrab()
    {
        if (!hasSelection) return;
        var target = interactablesSelected[0];
        interactionManager?.SelectExit(this, target);
    }
}