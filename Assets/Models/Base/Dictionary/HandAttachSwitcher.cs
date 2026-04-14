using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class HandAttachSwitcher : MonoBehaviour
{
    public Transform rightAttach;
    public Transform leftAttach;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnSelect);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject.transform;

        // грубая проверка: левая или правая рука
        if (interactor.name.Contains("hand_L"))
        {
            grab.attachTransform = leftAttach;
        }
        else
        {
            grab.attachTransform = rightAttach;
        }
    }
}