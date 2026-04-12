using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class PinchGrabInteractor : XRBaseInteractor
{
    [Header("🔹 Настройки для разных типов захвата")]
    public GameObject pinchVisual;
    public GameObject fistVisual;
    
    private GrabType currentGrabType = GrabType.Pinch; // ← Используем общий тип!
    private IXRSelectInteractable currentInteractable;

    public bool isGrabbing => hasSelection;

    public void StartGrab(IXRSelectInteractable interactable, GrabType grabType)
    {
        if (hasSelection || !CanSelect(interactable)) return;
        
        currentGrabType = grabType;
        currentInteractable = interactable;
        UpdateGrabVisuals();

        if (interactionManager != null)
            interactionManager.SelectEnter(this, interactable);
    }

    public void StopGrab()
    {
        if (!hasSelection || interactablesSelected.Count == 0) return;

        var target = interactablesSelected[0];

        currentInteractable = null;
        UpdateGrabVisuals();

        if (interactionManager != null)
            interactionManager.SelectExit(this, target);
    }
    
    public void ChangeGrabType(GrabType newGrabType)
    {
        if (!hasSelection) return;

        currentGrabType = newGrabType;
        UpdateGrabVisuals();

        // ❗ НЕ трогаем InteractionManager
        // Просто меняем attachTransform — этого достаточно
    }
    
    void UpdateGrabVisuals()
    {
        if (pinchVisual != null) pinchVisual.SetActive(currentGrabType == GrabType.Pinch);
        if (fistVisual != null) fistVisual.SetActive(currentGrabType == GrabType.Fist);
    }
    
    // Переопределяем метод для сохранения attach pose при захвате
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        currentInteractable = args.interactableObject;
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        currentInteractable = null;
    }
}