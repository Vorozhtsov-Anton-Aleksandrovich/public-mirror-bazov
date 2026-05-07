using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DictionaryReturn : MonoBehaviour
{
    public float distance = 2f;
    public float moveSpeed = 5f;

    public Transform playerHead;
    public BeltSlot beltSlot;

    private XRGrabInteractable grab;
    private bool isReturning = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (grab.isSelected) return;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        if (dist >= distance && !isReturning)
        {
            StartReturn();
        }

        if (isReturning)
        {
            MoveToSlot();
        }
    }

    void StartReturn()
    {
        isReturning = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // отключаем физику
    }

    void MoveToSlot()
    {
        Transform target = beltSlot.slotPoint;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * moveSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            target.rotation,
            Time.deltaTime * moveSpeed
        );

        // когда почти долетели — фиксируем
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            isReturning = false;
            beltSlot.AttachItem(grab);
        }
    }
}