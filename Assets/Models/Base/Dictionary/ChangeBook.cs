using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChangeBook : MonoBehaviour
{
    public GameObject closedModel;
    public GameObject openModel;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        SetOpen(false); // по умолчанию закрыта
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        SetOpen(true);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        openModel.SetActive(open);
        closedModel.SetActive(!open);
    }
}