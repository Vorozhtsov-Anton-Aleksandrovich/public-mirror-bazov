using UnityEngine;
using UnityEngine.Events;

public abstract class QuestStep : MonoBehaviour
{
    [Header("События шага")]
    public UnityEvent OnStepStarted;
    public UnityEvent OnStepCompleted;

    public bool isCompleted = false; // защита от двойного завершения

    public void StartStep()
    {
        if (gameObject.activeInHierarchy)
        {
            OnStepStarted?.Invoke();
        }
    }

    /// <summary>
    /// Завершает шаг, но только один раз.
    /// </summary>
    protected void FinishStep()
    {
        if (isCompleted) return; // уже завершён — выходим

        isCompleted = true;
        OnStepCompleted?.Invoke();
        QuestSequencer.Instance?.CompleteCurrentStep();
    }
}