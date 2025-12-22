using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Управляет последовательным выполнением шагов квеста.
/// Использует List вместо Queue для большей гибкости.
/// Поддерживает события:
/// - всего квеста (OnQuestStarted / OnQuestCompleted),
/// - каждого шага (OnStepStarted / OnStepCompleted).
/// </summary>
public class QuestSequencer : MonoBehaviour
{
    public static QuestSequencer Instance;

    // === События КВЕСТА ===
    [Header("События квеста")]
    public UnityEvent OnQuestStarted;
    public UnityEvent OnQuestCompleted;

    // === СПИСОК ШАГОВ ===
    [Header("Список шагов (порядок выполнения)")]
    [Tooltip("Перетащите сюда шаги в нужном порядке. Все шаги должны быть изначально ВЫКЛЮЧЕНЫ.")]
    public List<QuestStep> steps = new List<QuestStep>();

    private int currentStepIndex = -1;
    private bool questStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        Instance = this;

        // Гарантируем, что все шаги выключены при старте
        foreach (var step in steps)
        {
            if (step != null)
                step.gameObject.SetActive(false);
        }

        // Запускаем первый шаг, если есть
        TryStartNextStep();
    }

    /// <summary>
    /// Вызывается из шага после его завершения.
    /// Продвигает индекс и запускает следующий шаг (если есть).
    /// </summary>
    public void CompleteCurrentStep()
    {
        TryStartNextStep();
    }

    /// <summary>
    /// Пытается запустить следующий шаг по индексу.
    /// Если это первый шаг — вызывает OnQuestStarted.
    /// Если шагов больше нет — вызывает OnQuestCompleted.
    /// </summary>
    private void TryStartNextStep()
    {
        print($"List<QuestStep> Count: {steps.Count}\nActual: {currentStepIndex + 1}");
        currentStepIndex++;

        if (currentStepIndex < steps.Count)
        {
            var step = steps[currentStepIndex];
            if (step != null)
            {
                step.gameObject.SetActive(true);
                step.StartStep(); // ← вызывает OnStepStarted

                if (!questStarted)
                {
                    questStarted = true;
                    OnQuestStarted?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"Шаг с индексом {currentStepIndex} — null!");
                Invoke(nameof(TryStartNextStep), 0.5f);
            }
        }
        else
        {
            // Квест завершён
            OnQuestCompleted?.Invoke();
        }
    }
}