using UnityEngine;

/// <summary>
/// Абстрактный базовый класс для любого шага квеста.
/// Любой конкретный шаг (доставка, стояние и т.д.) должен наследоваться от этого класса
/// и реализовывать метод Complete().
/// </summary>
public abstract class QuestStep : MonoBehaviour
{
    /// <summary>
    /// Вызывается, когда шаг успешно завершён.
    /// Должен уведомить QuestSequencer о завершении.
    /// </summary>
    public abstract void Complete();
}
