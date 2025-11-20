using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// Телепортация через жест руки с использованием XR Interaction Toolkit.
/// Использует стандартный TeleportRequested → полностью совместим с Locomotion System.
/// </summary>
public class HandRecognizer : MonoBehaviour
{
    [Header("🔹 Жест и сжатие")]
    public XRHandShape pointingGesture;
    public XRHandJointID[] fingersToSqueeze = { XRHandJointID.IndexDistal, XRHandJointID.MiddleDistal };
    public float squeezeThreshold = 0.08f;
    public int confirmationFrames = 2;

    [Header("🔹 Телепортация")]
    public LayerMask teleportLayerMask = 1; // ← обязательно настрой!
    public float maxTeleportDistance = 10f;
    public GameObject teleportPreviewPrefab; // опционально

    [Header("🔹 Ссылки")]
    public XROrigin xrOrigin; // ← должен быть назначен!
    public XRHandTrackingEvents leftHandEvents;
    public XRHandTrackingEvents rightHandEvents;

    // ДОБАВЛЯЕМ: ссылку на Teleportation Provider (из XRI 3.0+)
    [Header("🔹 Teleportation Provider (XRI 3.0+)")]
    public TeleportationProvider teleportationProvider; // ← ПРИКРЕПИ ЭТО В ИНСПЕКТОРЕ!

    // ─────────────────────────────────────────────
    // СОСТОЯНИЯ И КЭШ
    // ─────────────────────────────────────────────

    private enum HandState { Idle, Pointing, AwaitingSqueeze, Triggered }
    
    private HandState leftState = HandState.Idle;
    private HandState rightState = HandState.Idle;
    private int leftConfirm = 0, rightConfirm = 0;

    private GameObject leftPreview, rightPreview;
    private Vector3? leftTarget, rightTarget;

    // ─────────────────────────────────────────────
    // ПОДПИСКА
    // ─────────────────────────────────────────────

    void OnEnable()
    {
        DebugInfoStaticController.ToTerminalQuoe("🔄 Hand Recognizer включён");

        if (leftHandEvents == null)
            DebugInfoStaticController.ToTerminalQuoe("❌ Left Hand Events не назначен!");
        else
            DebugInfoStaticController.ToTerminalQuoe($"✅ Left Hand Events: {leftHandEvents.name}");

        if (rightHandEvents == null)
            DebugInfoStaticController.ToTerminalQuoe("❌ Right Hand Events не назначен!");
        else
            DebugInfoStaticController.ToTerminalQuoe($"✅ Right Hand Events: {rightHandEvents.name}");

        if (leftHandEvents) leftHandEvents.jointsUpdated.AddListener(OnLeftHand);
        if (rightHandEvents) rightHandEvents.jointsUpdated.AddListener(OnRightHand);
        if (leftHandEvents) leftHandEvents.jointsUpdated.AddListener(OnLeftHand);
        if (rightHandEvents) rightHandEvents.jointsUpdated.AddListener(OnRightHand);
        
    }

    void OnDisable()
    {
        if (leftHandEvents) leftHandEvents.jointsUpdated.RemoveListener(OnLeftHand);
        if (rightHandEvents) rightHandEvents.jointsUpdated.RemoveListener(OnRightHand);
        Cleanup();
    }

    void Cleanup()
    {
        if (leftPreview) Destroy(leftPreview);
        if (rightPreview) Destroy(rightPreview);
        leftTarget = rightTarget = null;
    }

    // ─────────────────────────────────────────────
    // ОБРАБОТКА РУК (БЕЗ REF!)
    // ─────────────────────────────────────────────

    void OnLeftHand(XRHandJointsUpdatedEventArgs args) => ProcessHand(args, true);
    void OnRightHand(XRHandJointsUpdatedEventArgs args) => ProcessHand(args, false);

   void ProcessHand(XRHandJointsUpdatedEventArgs args, bool isLeft)
    {
        DebugInfoStaticController.ToTerminalQuoe($"🔄 Обработка {(isLeft ? "левой" : "правой")} руки");

        // Используем обычные переменные вместо ref — чтобы избежать CS1510
        HandState state = isLeft ? leftState : rightState;
        int confirm = isLeft ? leftConfirm : rightConfirm;
        GameObject preview = isLeft ? leftPreview : rightPreview;
        Vector3? target = isLeft ? leftTarget : rightTarget;

        var hand = args.hand;
        if (!hand.isTracked)
        {
            DebugInfoStaticController.ToTerminalQuoe($"❌ Рука {(isLeft ? "левая" : "правая")} не трекируется");
            if (isLeft) { leftState = HandState.Idle; leftConfirm = 0; }
            else { rightState = HandState.Idle; rightConfirm = 0; }
            if (preview) Destroy(preview);
            if (isLeft) leftTarget = null; else rightTarget = null;
            return;
        }

        DebugInfoStaticController.ToTerminalQuoe($"✅ Рука {(isLeft ? "левая" : "правая")} трекируется");

        switch (state)
        {
            case HandState.Idle:
                if (pointingGesture && pointingGesture.CheckConditions(args))
                {
                    DebugInfoStaticController.ToTerminalQuoe($"✅ ЖЕСТ РАСПОЗНАН: {(isLeft ? "ЛЕВАЯ" : "ПРАВАЯ")} рука");
                    if (TryGetTeleportTarget(hand, out Vector3 hitPoint))
                    {
                        target = hitPoint;
                        DebugInfoStaticController.ToTerminalQuoe($"🎯 Цель найдена: {hitPoint}");
                        if (teleportPreviewPrefab)
                            preview = Instantiate(teleportPreviewPrefab, hitPoint, Quaternion.identity);
                    }
                    else
                    {
                        target = null;
                        preview = null;
                        DebugInfoStaticController.ToTerminalQuoe("❌ Цель НЕ найдена");
                    }
                    state = HandState.Pointing;
                }
                break;

            case HandState.Pointing:
                if (pointingGesture.CheckConditions(args))
                {
                    DebugInfoStaticController.ToTerminalQuoe($"🟩 Жест продолжается");
                    if (TryGetTeleportTarget(hand, out Vector3 hitPoint))
                    {
                        target = hitPoint;
                        DebugInfoStaticController.ToTerminalQuoe($"🎯 Цель обновлена: {hitPoint}");
                        if (teleportPreviewPrefab)
                        {
                            if (preview == null)
                                preview = Instantiate(teleportPreviewPrefab, hitPoint, Quaternion.identity);
                            else
                                preview.transform.position = hitPoint;
                        }
                    }
                    else
                    {
                        target = null;
                        if (preview) Destroy(preview);
                        DebugInfoStaticController.ToTerminalQuoe("❌ Цель потеряна");
                    }
                }
                else
                {
                    DebugInfoStaticController.ToTerminalQuoe($"🔴 Жест завершён → ожидаем сжатие");
                    state = HandState.AwaitingSqueeze;
                    confirm = 0;
                }
                break;

            case HandState.AwaitingSqueeze:
                if (AreFingersSqueezed(hand, fingersToSqueeze, squeezeThreshold))
                {
                    confirm++;
                    DebugInfoStaticController.ToTerminalQuoe($"🟡 Сжатие: {confirm}/{confirmationFrames}");
                    if (confirm >= confirmationFrames && target.HasValue)
                    {
                        DebugInfoStaticController.ToTerminalQuoe($"🔥 ТЕЛЕПОРТАЦИЯ В {target.Value}");
                        TeleportTo(target.Value);
                        state = HandState.Triggered;
                        confirm = 0;
                        if (preview) Destroy(preview);
                        target = null;
                    }
                }
                else
                {
                    DebugInfoStaticController.ToTerminalQuoe($"🔵 Сжатие отменено");
                    state = HandState.Idle;
                    confirm = 0;
                    if (preview) Destroy(preview);
                    target = null;
                }
                break;

            case HandState.Triggered:
                if (!AreFingersSqueezed(hand, fingersToSqueeze, squeezeThreshold))
                {
                    DebugInfoStaticController.ToTerminalQuoe($"🟢 Сброс состояния");
                    state = HandState.Idle;
                }
                break;
        }

        // Сохраняем изменения обратно в поля
        if (isLeft)
        {
            leftState = state;
            leftConfirm = confirm;
            leftPreview = preview;
            leftTarget = target;
        }
        else
        {
            rightState = state;
            rightConfirm = confirm;
            rightPreview = preview;
            rightTarget = target;
        }
    }

    // ─────────────────────────────────────────────
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ─────────────────────────────────────────────

    bool TryGetTeleportTarget(XRHand hand, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        if (!TryGetPointingRay(hand, out Ray ray))
            return false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxTeleportDistance, teleportLayerMask))
        {
            hitPoint = hit.point;
            return true;
        }

        return false;
    }

    bool TryGetPointingRay(XRHand hand, out Ray ray)
    {
        ray = default;

        var wrist = hand.GetJoint(XRHandJointID.Wrist);
        var index = hand.GetJoint(XRHandJointID.IndexDistal);
        var middle = hand.GetJoint(XRHandJointID.MiddleDistal);

        bool wristOK = wrist.TryGetPose(out Pose w);
        bool indexOK = index.TryGetPose(out Pose i);
        bool middleOK = middle.TryGetPose(out Pose m);

        if (!wristOK)
            DebugInfoStaticController.ToTerminalQuoe(">    Запястье не отслеживается");
        if (!indexOK)
            DebugInfoStaticController.ToTerminalQuoe(">    Указательный палец не отслеживается");
        if (!middleOK)
            DebugInfoStaticController.ToTerminalQuoe(">    Средний палец не отслеживается");

        if (!wristOK || !indexOK || !middleOK)
        {
            return false;
        }

        Vector3 origin = w.position;
        Vector3 direction = ((i.position + m.position) * 0.5f - origin).normalized;
        ray = new Ray(origin, direction);

        Debug.DrawRay(origin, direction * 10f, Color.red, 0.1f);
        return true;
    }

    bool AreFingersSqueezed(XRHand hand, XRHandJointID[] tips, float threshold)
    {
        if (!hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palm))
            return false;

        foreach (var tip in tips)
        {
            if (!hand.GetJoint(tip).TryGetPose(out Pose t) || 
                Vector3.Distance(t.position, palm.position) > threshold)
                return false;
        }
        return true;
    }

    void TeleportTo(Vector3 targetPosition)
    {
        if (teleportationProvider == null)
        {
            DebugInfoStaticController.ToTerminalQuoe("Teleportation Provider не назначен! Перетащи его в инспекторе.");
            return;
        }

        // Получаем текущую ориентацию игрока
        Quaternion playerRotation = xrOrigin.transform.rotation;

        // Создаём запрос телепортации
        var request = new TeleportRequest
        {
            destinationPosition = targetPosition,
            destinationRotation = playerRotation
        };

        // Вызываем телепортацию через XRI 3.0+
        teleportationProvider.QueueTeleportRequest(request);
    }
}
