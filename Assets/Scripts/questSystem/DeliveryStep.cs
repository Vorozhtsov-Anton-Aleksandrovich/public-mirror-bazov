using UnityEngine;

/// <summary>
/// Шаг квеста: игрок должен доставить указанный предмет в зону доставки.
/// Завершается автоматически, когда предмет попадает в DeliveryZone.
/// </summary>
public class DeliveryStep : QuestStep
{
    [Tooltip("Предмет, который игрок должен доставить (GameObject в сцене или prefab)")]
    public GameObject targetItem;

    [Tooltip("Зона доставки — объект с компонентом DeliveryZone")]
    public DeliveryZone deliveryZone;

    private void OnEnable()
    {
        // Подписываемся на событие доставки
        if (deliveryZone != null)
            deliveryZone.OnDelivered += OnItemDelivered;
    }

    private void OnDisable()
    {
        // Отписываемся при выключении (важно для корректности)
        if (deliveryZone != null)
            deliveryZone.OnDelivered -= OnItemDelivered;
    }

    private void OnItemDelivered(GameObject deliveredItem)
    {
        // Проверяем: тот ли это предмет?
        if (deliveredItem == targetItem)
        {
            Complete(); // Уведомляем систему о завершении
        }
    }

    public override void Complete()
    {
        Debug.Log($"[DeliveryStep] Завершён: {name}");
        QuestSequencer.Instance?.CompleteCurrentStep();
    }
}