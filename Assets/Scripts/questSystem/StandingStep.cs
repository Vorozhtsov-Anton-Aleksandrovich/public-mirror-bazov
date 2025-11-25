using UnityEngine;

public class StandingStep : QuestStep
{
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
        FinishStep(); // завершает шаг
    }
}