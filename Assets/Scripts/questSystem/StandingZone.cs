using UnityEngine;
using System;

/// <summary>
/// Невидимая зона на полу или в пространстве.
/// Срабатывает, когда игрок (с тегом "Player") входит в неё.
/// </summary>
public class StandingZone : QuestStep
{
    [Tooltip("Вызывается, когда игрок входит в зону")]
    public event Action OnPlayerEntered;

    private void OnTriggerEnter(Collider other)
    {
        print($"Player - {other.gameObject.tag}: {other.gameObject.name}");
        // Проверяем, что это игрок
        if (other.gameObject.tag == "Player")
        {
            OnPlayerEntered?.Invoke();
            FinishStep();
        }
    }
}