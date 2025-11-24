using UnityEngine;

public class DeliveryStep : QuestStep
{
    public string tagTargetItem;
    public DeliveryZone deliveryZone;

    private void OnEnable()
    {
        if (deliveryZone != null)
            deliveryZone.OnDelivered += OnItemDelivered;
    }

    private void OnDisable()
    {
        if (deliveryZone != null)
            deliveryZone.OnDelivered -= OnItemDelivered;
    }

    private void OnItemDelivered(GameObject deliveredItem)
    {
        if (deliveredItem.tag == tagTargetItem)
        {
            deliveredItem.SetActive(false);
            FinishStep(); // ← завершает шаг и вызывает OnStepCompleted
        }
    }
}