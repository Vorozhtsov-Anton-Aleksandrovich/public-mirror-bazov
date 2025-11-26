using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Hands;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRHandSkeletonDriver))]
[RequireComponent(typeof(PinchGrabInteractor))]
public class HandGrabController : MonoBehaviour
{
    [Header("🔹 Щепотка (два пальца)")]
    public float pinchThreshold = 0.03f; // 3 cm
    public Transform pinchAttachPoint; // Точка захвата для щепотки

    [Header("🔹 Кулак (вся рука)")]
    public float fistThreshold = 0.08f; // 8 cm
    public Transform fistAttachPoint; // Точка захвата для кулака
    [Tooltip("Какие пальцы учитывать для распознавания кулака")]
    public XRHandJointID[] fistFingers = { 
        XRHandJointID.ThumbTip, 
        XRHandJointID.IndexTip, 
        XRHandJointID.MiddleTip,
        XRHandJointID.RingTip,
        XRHandJointID.LittleTip
    };
    
    [Header("🔹 Общие настройки")]
    public bool prioritizePinchOverFist = true; // Приоритет щепотки над кулаком
    public float releaseThresholdMultiplier = 1.2f; // Мультипликатор для гистерезиса

    private XRHandSkeletonDriver skeletonDriver;
    private PinchGrabInteractor grabInteractor;
    private IXRSelectInteractable currentHoverTarget;
    private GrabType currentGrabType = GrabType.None; // ← Используем общий тип!
    
    // Для гистерезиса (плавного перехода между состояниями)
    private float currentPinchThreshold => pinchThreshold * (currentGrabType == GrabType.Pinch ? releaseThresholdMultiplier : 1f);
    private float currentFistThreshold => fistThreshold * (currentGrabType == GrabType.Fist ? releaseThresholdMultiplier : 1f);

    void Awake()
    {
        skeletonDriver = GetComponent<XRHandSkeletonDriver>();
        grabInteractor = GetComponent<PinchGrabInteractor>();
        
        // Создаем точки захвата по умолчанию, если они не заданы
        if (pinchAttachPoint == null)
            pinchAttachPoint = CreateDefaultAttachPoint("PinchAttachPoint");
        
        if (fistAttachPoint == null)
            fistAttachPoint = CreateDefaultAttachPoint("FistAttachPoint");
    }

    Transform CreateDefaultAttachPoint(string name)
    {
        GameObject attachPoint = new GameObject(name);
        attachPoint.transform.SetParent(transform);
        attachPoint.transform.localPosition = Vector3.zero;
        return attachPoint.transform;
    }

    void Update()
    {
        UpdateHover();
        UpdateGrab();
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

    void UpdateGrab()
    {
        bool isPinching = IsHandPinching();
        bool isFisting = IsHandFisted();
        GrabType newGrabType = GrabType.None;
        
        // Определяем тип захвата по приоритету
        if (prioritizePinchOverFist)
        {
            if (isPinching) newGrabType = GrabType.Pinch;
            else if (isFisting) newGrabType = GrabType.Fist;
        }
        else
        {
            if (isFisting) newGrabType = GrabType.Fist;
            else if (isPinching) newGrabType = GrabType.Pinch;
        }

        // Захват объекта
        if (!grabInteractor.isGrabbing && newGrabType != GrabType.None && currentHoverTarget != null)
        {
            currentGrabType = newGrabType;
            SetAttachTransformForGrabType(currentGrabType);
            grabInteractor.StartGrab(currentHoverTarget, currentGrabType); // ← Передаём общий тип!
        }
        // Отпускание объекта
        else if (grabInteractor.isGrabbing && !isPinching && !isFisting)
        {
            grabInteractor.StopGrab();
            currentGrabType = GrabType.None;
        }
        // Смена типа захвата (если нужно)
        else if (grabInteractor.isGrabbing && newGrabType != currentGrabType && newGrabType != GrabType.None)
        {
            currentGrabType = newGrabType;
            SetAttachTransformForGrabType(currentGrabType);
            grabInteractor.ChangeGrabType(currentGrabType); // ← Передаём общий тип!
        }
    }
    
    void SetAttachTransformForGrabType(GrabType grabType)
    {
        if (grabType == GrabType.Pinch && pinchAttachPoint != null)
            grabInteractor.attachTransform = pinchAttachPoint;
        else if (grabType == GrabType.Fist && fistAttachPoint != null)
            grabInteractor.attachTransform = fistAttachPoint;
        else
            grabInteractor.attachTransform = transform; // fallback
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

        return Vector3.Distance(index.position, thumb.position) < currentPinchThreshold;
    }
    
    bool IsHandFisted()
    {
        Transform palm = GetJointTransform(XRHandJointID.Palm);
        if (palm == null) return false;
        
        foreach (var fingerTip in fistFingers)
        {
            Transform tip = GetJointTransform(fingerTip);
            if (tip == null) continue;
            
            // Для большого пальца используем другую логику
            if (fingerTip == XRHandJointID.ThumbTip)
            {
                if (Vector3.Distance(tip.position, palm.position) > currentFistThreshold * 1.5f)
                    return false;
            }
            else
            {
                if (Vector3.Distance(tip.position, palm.position) > currentFistThreshold)
                    return false;
            }
        }
        
        return true;
    }
}