using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class RotatingWheel : MonoBehaviour
{
    [Tooltip("Ось вращения руля (0,1,0 для горизонтального руля)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    
    [Tooltip("Скорость вращения")]
    [SerializeField] private float rotationMultiplier = 1f;
    
    private XRGrabInteractable grabInteractable;
    private Quaternion initialRotation;
    private Vector3 initialGrabDirection;
    private bool isGrabbed = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(_ => OnGrab());
        grabInteractable.deactivated.AddListener(_ => OnRelease());
    }

    void OnGrab()
    {
        isGrabbed = true;
        initialRotation = transform.rotation;
        initialGrabDirection = Vector3.zero;
    }

    void OnRelease()
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed || !grabInteractable.isSelected) return;

        var interactor = grabInteractable.GetOldestInteractorSelecting();
        if (interactor == null) return;

        // Направление от центра руля к контроллеру
        Vector3 toController = interactor.transform.position - transform.position;
        
        // Проекция на плоскость, перпендикулярную оси вращения
        Vector3 projected = Vector3.ProjectOnPlane(toController, rotationAxis).normalized;
        if (projected == Vector3.zero) return;

        // При первом кадре захвата запоминаем начальное направление
        if (initialGrabDirection == Vector3.zero)
        {
            initialGrabDirection = projected;
            return;
        }

        // Считаем угол поворота
        float angle = Vector3.SignedAngle(initialGrabDirection, projected, rotationAxis);
        
        // Применяем поворот
        transform.rotation = initialRotation * Quaternion.AngleAxis(angle * rotationMultiplier, rotationAxis);
    }
}