// StandingStep.cs
using UnityEngine;

/// <summary>
/// Шаг квеста: игрок должен встать в указанную зону (например, на алтарь).
/// Завершается автоматически при входе игрока в StandingZone.
/// </summary>
public class StandingStep : QuestStep
{
    [Tooltip("Зона, в которую игрок должен встать (объект с компонентом StandingZone)")]
    public StandingZone standingZone;

    private void OnEnable()
    {
        if (standingZone != null)
            standingZone.OnPlayerEntered += OnPlayerEntered;
    }

    private void OnDisable()
    {
        if (standingZone != null)
            standingZone.OnPlayerEntered -= OnPlayerEntered;
    }

    private void OnPlayerEntered()
    {
        Complete(); // Сразу завершаем — условие выполнено
    }

    public override void Complete()
    {
        Debug.Log($"[StandingStep] Завершён: {name}");
        QuestSequencer.Instance?.CompleteCurrentStep();
    }
}