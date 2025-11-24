using UnityEngine;
using System;

/// <summary>
/// Невидимая зона на полу или в пространстве.
/// Срабатывает, когда игрок (с тегом "Player") входит в неё.
/// </summary>
public class StandingZone : MonoBehaviour
{
    [Tooltip("Вызывается, когда игрок входит в зону")]
    public event Action OnPlayerEntered;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag("Player"))
        {
            OnPlayerEntered?.Invoke();
        }
    }
}