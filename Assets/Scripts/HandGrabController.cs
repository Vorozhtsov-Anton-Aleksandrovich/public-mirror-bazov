using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Hands;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRHandSkeletonDriver))]
[RequireComponent(typeof(PinchGrabInteractor))]
public class HandGrabController : MonoBehaviour
{
    [Header("Pinch Settings")]
    public float pinchThreshold = 0.03f; // 3 cm

    private XRHandSkeletonDriver skeletonDriver;
    private PinchGrabInteractor grabInteractor;
    private IXRSelectInteractable currentHoverTarget;

    void Awake()
    {
        skeletonDriver = GetComponent<XRHandSkeletonDriver>();
        grabInteractor = GetComponent<PinchGrabInteractor>();
    }

    void Update()
    {
        UpdateHover();
        UpdatePinch();
    }

    void UpdateHover()
    {
        Transform palm = GetJointTransform(XRHandJointID.Palm);
        Vector3 origin = palm != null ? palm.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, 0.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        IXRSelectInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var col in hits)
        {
            if (col.TryGetComponent<IXRSelectInteractable>(out var interactable))
            {
                float dist = Vector3.Distance(origin, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }
        }

        currentHoverTarget = closest;
    }

    void UpdatePinch()
    {
        bool isPinching = IsHandPinching();

        if (!grabInteractor.isGrabbing && isPinching && currentHoverTarget != null)
        {
            grabInteractor.StartGrab(currentHoverTarget);
        }
        else if (grabInteractor.isGrabbing && !isPinching)
        {
            grabInteractor.StopGrab();
        }
    }

    Transform GetJointTransform(XRHandJointID jointId)
    {
        return skeletonDriver.jointTransformReferences
            .FirstOrDefault(r => r.xrHandJointID == jointId).jointTransform;
    }

    bool IsHandPinching()
    {
        Transform index = GetJointTransform(XRHandJointID.IndexTip);
        Transform thumb = GetJointTransform(XRHandJointID.ThumbTip);
        if (index == null || thumb == null) return false;

        return Vector3.Distance(index.position, thumb.position) < pinchThreshold;
    }
}