using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BeltSlot : MonoBehaviour
{
    public Transform slotPoint;

    private XRGrabInteractable currentItem;

    private void OnTriggerEnter(Collider other)
    {
        if (currentItem != null) return;

        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null) return;

        // Проверяем, что объект НЕ в руке
        if (grab.isSelected) return;

        AttachItem(grab);
    }

    void AttachItem(XRGrabInteractable item)
    {
        currentItem = item;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // Отключаем взаимодействие временно
        //item.enabled = false;

        // Привязываем к слоту
        item.transform.SetPositionAndRotation(slotPoint.position, slotPoint.rotation);

        item.GetComponent<ChangeBook>()?.SetOpen(false);
    }

    public void ForceDetach()
    {
        if (currentItem == null) return;

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        currentItem = null;

        currentItem.GetComponent<ChangeBook>()?.SetOpen(true);
    }

    private void LateUpdate()
    {
        if (currentItem == null) return;

        // ❗ ЕСЛИ XR забрал объект — слот должен отпустить его
        if (currentItem.isSelected)
        {
            ForceDetach();
            return;
        }

        currentItem.transform.SetPositionAndRotation(
            slotPoint.position,
            slotPoint.rotation
        );
    }
}