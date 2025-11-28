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
    // Предопределенный жест, который распознается как "указание" (обычно указательный палец вытянут)
    public XRHandShape pointingGesture;
    
    // Пальцы, которые должны быть сжаты для активации телепортации
    // По умолчанию - указательный и средний пальцы в дистальной точке
    public XRHandJointID[] fingersToSqueeze = { XRHandJointID.IndexDistal, XRHandJointID.MiddleDistal };
    
    // Порог расстояния между кончиками пальцев и ладонью для определения "сжатия"
    // Если расстояние меньше этого порога - считаем, что пальцы сжаты
    public float squeezeThreshold = 0.08f;
    
    // Количество кадров, в течение которых должно сохраняться условие сжатия
    // Это предотвращает случайные срабатывания телепортации
    public int confirmationFrames = 2;

    [Header("🔹 Телепортация")]
    // Слой, по которому можно телепортироваться (пол, платформы)
    // ВАЖНО: нужно настроить в инспекторе, обычно это "Default" или "Ground"
    public LayerMask teleportLayerMask = 1;
    
    // Максимальная дистанция телепортации
    public float maxTeleportDistance = 10f;
    
    // Префаб, который отображается в точке телепортации (курсор, подсветка)
    public GameObject teleportPreviewPrefab;

    [Header("🔹 Ссылки")]
    // Ссылка на XROrigin - корневой объект VR-камеры и контроллеров
    public XROrigin xrOrigin;
    
    // События обновления суставов левой и правой рук
    // Используются для получения данных о положении пальцев
    public XRHandTrackingEvents leftHandEvents;
    public XRHandTrackingEvents rightHandEvents;

    // Ссылка на TeleportationProvider из XR Interaction Toolkit v3.0+
    // Отвечает за обработку запросов на телепортацию
    [Header("🔹 Teleportation Provider (XRI 3.0+)")]
    public TeleportationProvider teleportationProvider;

    // ─────────────────────────────────────────────
    // СОСТОЯНИЯ И КЭШ
    // ─────────────────────────────────────────────

    // Состояния обработки жестов для каждой руки
    private enum HandState { 
        Idle,           // Рука неактивна, жест не распознается
        Pointing,       // Распознан жест указания (указательный палец вытянут)
        AwaitingSqueeze,// Ожидание сжатия пальцев для подтверждения телепортации
        Triggered       // Телепортация активирована, ожидание разжатия пальцев
    }
    
    // Текущие состояния для левой и правой руки
    private HandState leftState = HandState.Idle;
    private HandState rightState = HandState.Idle;
    
    // Счетчики для подтверждения жеста (должен продержаться указанное количество кадров)
    private int leftConfirm = 0, rightConfirm = 0;

    // Префабы-превью телепортации для каждой руки
    private GameObject leftPreview, rightPreview;
    
    // Точки телепортации для каждой руки (null, если нет валидной цели)
    private Vector3? leftTarget, rightTarget;

    // ─────────────────────────────────────────────
    // ПОДПИСКА
    // ─────────────────────────────────────────────

    /// <summary>
    /// Подписка на события обновления суставов рук при активации объекта
    /// </summary>
    void OnEnable()
    {
        if (leftHandEvents) leftHandEvents.jointsUpdated.AddListener(OnLeftHand);
        if (rightHandEvents) rightHandEvents.jointsUpdated.AddListener(OnRightHand);
    }

    /// <summary>
    /// Отписка от событий и очистка при деактивации объекта
    /// </summary>
    void OnDisable()
    {
        if (leftHandEvents) leftHandEvents.jointsUpdated.RemoveListener(OnLeftHand);
        if (rightHandEvents) rightHandEvents.jointsUpdated.RemoveListener(OnRightHand);
        Cleanup();
    }

    /// <summary>
    /// Удаляет превью телепортации и сбрасывает целевые точки
    /// </summary>
    void Cleanup()
    {
        if (leftPreview) Destroy(leftPreview);
        if (rightPreview) Destroy(rightPreview);
        leftTarget = rightTarget = null;
    }

    // ─────────────────────────────────────────────
    // ОБРАБОТКА РУК (БЕЗ REF!)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Обработчики событий обновления суставов для левой и правой руки
    /// Направляют данные в общий метод обработки
    /// </summary>
    void OnLeftHand(XRHandJointsUpdatedEventArgs args) => ProcessHand(args, true);
    void OnRightHand(XRHandJointsUpdatedEventArgs args) => ProcessHand(args, false);

    /// <summary>
    /// Основная логика обработки жестов для руки
    /// </summary>
    /// <param name="args">Данные об обновлении суставов руки</param>
    /// <param name="isLeft">true для левой руки, false для правой</param>
    void ProcessHand(XRHandJointsUpdatedEventArgs args, bool isLeft)
    {
        // Получаем текущее состояние руки
        HandState state = isLeft ? leftState : rightState;
        int confirm = isLeft ? leftConfirm : rightConfirm;
        GameObject preview = isLeft ? leftPreview : rightPreview;
        Vector3? target = isLeft ? leftTarget : rightTarget;

        var hand = args.hand;
        
        // Если рука не отслеживается, сбрасываем состояние
        if (!hand.isTracked)
        {
            if (isLeft) { leftState = HandState.Idle; leftConfirm = 0; }
            else { rightState = HandState.Idle; rightConfirm = 0; }
            
            // Удаляем превью телепортации
            if (preview) Destroy(preview);
            if (isLeft) leftTarget = null; else rightTarget = null;
            return;
        }

        // Проверяем вертикальную скорость запястья (костыль для определения направления руки)
        // Использует внутреннюю иерархию объектов для получения трансформа запястья
        float up_velocity = isLeft ? 
            leftHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("L_Wrist").transform.up.y : 
            rightHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("R_Wrist").transform.up.y;

        // Конечный автомат обработки состояний руки
        switch (state)
        {
            case HandState.Idle:
                // Проверяем, распознается ли жест указания и направлен ли жест вниз
                if (pointingGesture && pointingGesture.CheckConditions(args) && up_velocity < -0.1f)
                {
                    // Пытаемся найти точку телепортации
                    if (TryGetTeleportTarget(hand, out Vector3 hitPoint, isLeft))
                    {
                        target = hitPoint;
                        // Создаем превью телепортации
                        if (teleportPreviewPrefab)
                            preview = Instantiate(teleportPreviewPrefab, hitPoint, Quaternion.identity);
                    }
                    else
                    {
                        target = null;
                        preview = null;
                    }
                    state = HandState.Pointing; // Переходим в состояние указания
                }
                break;

            case HandState.Pointing:
                // Если жест указания все еще активен
                if (pointingGesture.CheckConditions(args))
                {
                    // Обновляем превью телепортации
                    if (TryGetTeleportTarget(hand, out Vector3 hitPoint, isLeft))
                    {
                        target = hitPoint;
                        if (teleportPreviewPrefab)
                        {
                            // Создаем превью, если его нет, или обновляем позицию
                            if (preview == null)
                                preview = Instantiate(teleportPreviewPrefab, hitPoint, Quaternion.identity);
                            else
                                preview.transform.position = hitPoint;
                        }
                    }
                    else
                    {
                        // Если точки телепортации нет - удаляем превью
                        target = null;
                        if (preview) Destroy(preview);
                    }
                }
                else
                {
                    // Жест указания прекратился, переходим к ожиданию сжатия
                    state = HandState.AwaitingSqueeze;
                    confirm = 0;
                }
                break;

            case HandState.AwaitingSqueeze:
                // Проверяем, сжаты ли пальцы
                if (AreFingersSqueezed(hand, fingersToSqueeze, squeezeThreshold))
                {
                    confirm++;
                    // Если сжатие подтверждено достаточное количество кадров и есть цель
                    if (confirm >= confirmationFrames && target.HasValue)
                    {
                        TeleportTo(target.Value); // Выполняем телепортацию
                        state = HandState.Triggered;
                        confirm = 0;
                        // Удаляем превью после телепортации
                        if (preview) Destroy(preview);
                        target = null;
                    }
                }
                else
                {
                    // Если пальцы разжаты - возвращаемся в исходное состояние
                    state = HandState.Idle;
                    confirm = 0;
                    if (preview) Destroy(preview);
                    target = null;
                }
                break;

            case HandState.Triggered:
                // Ожидаем разжатия пальцев для возврата в исходное состояние
                if (!AreFingersSqueezed(hand, fingersToSqueeze, squeezeThreshold))
                {
                    state = HandState.Idle;
                }
                break;
        }

        // Сохраняем обновленные значения в поля класса
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

    /// <summary>
    /// Пытается найти точку телепортации, но блокирует телепортацию, если луч пересекает
    /// объект, НЕ входящий в teleportLayerMask, до целевой точки.
    /// </summary>
    bool TryGetTeleportTarget(XRHand hand, out Vector3 hitPoint, bool isLeft)
    {
        hitPoint = Vector3.zero;

        if (!TryGetPointingRay(hand, out Ray ray, isLeft))
        {
            return false;
        }

        // Получаем ВСЕ пересечения вдоль луча
        RaycastHit[] allHits = Physics.RaycastAll(ray, maxTeleportDistance);

        // Сортируем по расстоянию (ближайшие первыми)
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        // Ищем первую точку, подходящую для телепортации
        foreach (RaycastHit hit in allHits)
        {
            int hitLayer = 1 << hit.collider.gameObject.layer;

            // Если объект НЕ входит в teleportLayerMask — он блокирует телепортацию
            if ((teleportLayerMask.value & hitLayer) == 0)
            {
                // Объект не для телепортации → блокируем всё
                return false;
            }

            // Если объект ВХОДИТ в teleportLayerMask — это наша цель
            hitPoint = hit.point;
            return true;
        }

        // Ничего не найдено
        return false;
    }

    /// <summary>
    /// Формирует луч указания на основе позиции запястья и пальцев
    /// </summary>
    bool TryGetPointingRay(XRHand hand, out Ray ray, bool isLeft)
    {
        // Рисуем отладочные лучи для визуализации в редакторе
        if (!isLeft)
            Debug.DrawRay(rightHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("R_Wrist").transform.position, 
                rightHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("R_Wrist").transform.forward * 10f, Color.yellow, 0.1f);
        else
            Debug.DrawRay(leftHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("L_Wrist").transform.position, 
                leftHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("L_Wrist").transform.forward * 10f, Color.yellow, 0.1f);
        
        ray = default;

        // Получаем позы ключевых суставов
        var wrist = hand.GetJoint(XRHandJointID.Wrist);
        var index = hand.GetJoint(XRHandJointID.IndexProximal);
        var middle = hand.GetJoint(XRHandJointID.MiddleProximal);

        // Проверяем, доступны ли данные о суставах
        if (!wrist.TryGetPose(out Pose w) || 
            !index.TryGetPose(out Pose i) || 
            !middle.TryGetPose(out Pose m))
        {
            return false;
        }

        // ЗАККОМЕНЧЕННЫЙ УЛУЧШЕННЫЙ АЛГОРИТМ:
        // Vector3 origin = w.position; // Позиция запястья
        // Vector3 tipCenter = (i.position + m.position) * 0.5f; // Центр между пальцами
        // Vector3 direction = (tipCenter - origin).normalized; // Направление от запястья к пальцам
        // direction.y -= 0.5f; // Наклон вниз для лучшего попадания в пол
        // direction.Normalize();

        // ТЕКУЩИЙ АЛГОРИТМ (использует forward-направление запястья):
        if (!isLeft)
            ray = new Ray(rightHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("R_Wrist").transform.position, 
                rightHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("R_Wrist").transform.forward);
        else
            ray = new Ray(leftHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("L_Wrist").transform.position, 
                leftHandEvents.gameObject.GetNamedChild("Armature").GetNamedChild("L_Wrist").transform.forward);

        return true;
    }

    /// <summary>
    /// Проверяет, сжаты ли указанные пальцы относительно ладони
    /// </summary>
    bool AreFingersSqueezed(XRHand hand, XRHandJointID[] tips, float threshold)
    {
        // Получаем позу ладони
        if (!hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palm))
            return false;

        // Проверяем каждый указанный палец
        foreach (var tip in tips)
        {
            // Если не можем получить позу пальца или расстояние до ладони больше порога
            if (!hand.GetJoint(tip).TryGetPose(out Pose t) || 
                Vector3.Distance(t.position, palm.position) > threshold)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Выполняет телепортацию в указанную точку через TeleportationProvider
    /// </summary>
    void TeleportTo(Vector3 targetPosition)
    {
        if (teleportationProvider == null)
        {
            return;
        }

        // Сохраняем текущую ориентацию игрока
        Quaternion playerRotation = xrOrigin.transform.rotation;

        // Создаем запрос на телепортацию
        var request = new TeleportRequest
        {
            destinationPosition = targetPosition,
            destinationRotation = playerRotation
        };

        // Отправляем запрос через систему телепортации XRI
        teleportationProvider.QueueTeleportRequest(request);
    }
}