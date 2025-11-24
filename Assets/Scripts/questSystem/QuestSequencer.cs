using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Управляет последовательным выполнением шагов квеста.
/// Хранит очередь шагов и автоматически активирует следующий после завершения текущего.
/// Поддерживает события начала и завершения всего квеста для подключения анимаций, звуков и т.п.
/// </summary>
public class QuestSequencer : MonoBehaviour
{
    // Синглтон для удобного доступа (опционально, но удобно на этом этапе)
    public static QuestSequencer Instance;

    // === СОБЫТИЯ КВЕСТА ===
    [Header("События квеста")]
    [Tooltip("Вызывается при активации первого шага (начало квеста)")]
    public UnityEvent OnQuestStarted;

    [Tooltip("Вызывается после завершения последнего шага (конец квеста)")]
    public UnityEvent OnQuestCompleted;

    // === ОЧЕРЕДЬ ШАГОВ ===
    [Header("Очередь шагов")]
    [Tooltip("Перетащите сюда GameObject'ы с компонентами DeliveryStep или StandingStep в нужном порядке.\n" +
             "Все шаги должны быть изначально ВЫКЛЮЧЕНЫ (SetActive(false)).")]
    public Queue<QuestStep> stepQueue = new Queue<QuestStep>();

    private bool hasStarted = false; // Флаг, чтобы не вызывать OnQuestStarted дважды

    private void Awake()
    {
        // Гарантируем единственный экземпляр
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Пытаемся запустить первый шаг
        TryStartNextStep();
    }

    /// <summary>
    /// Вызывается из любого завершённого шага.
    /// Продвигает очередь и запускает следующий шаг (если есть).
    /// </summary>
    public void CompleteCurrentStep()
    {
        TryStartNextStep();
    }

    /// <summary>
    /// Пытается запустить следующий шаг из очереди.
    /// Если шагов больше нет — вызывает OnQuestCompleted.
    /// Если это первый шаг — вызывает OnQuestStarted.
    /// </summary>
    private void TryStartNextStep()
    {
        if (stepQueue.Count > 0)
        {
            var nextStep = stepQueue.Dequeue();

            if (nextStep != null)
            {
                nextStep.gameObject.SetActive(true); // Активируем шаг

                if (!hasStarted)
                {
                    hasStarted = true;
                    OnQuestStarted?.Invoke(); // Квест начался!
                }
            }
            else
            {
                Debug.LogWarning("Обнаружен null-шаг в очереди квеста!");
            }
        }
        else
        {
            // Квест полностью завершён
            OnQuestCompleted?.Invoke();
        }
    }
}
