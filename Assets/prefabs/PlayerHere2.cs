using UnityEngine;

public class PlayerHere2 : MonoBehaviour
{
[SerializeField] private Animator[] targetAnimators; // Массив аниматоров целевых объектов (не игроков)
    [SerializeField] private string parameterName = "PlayerHere";
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            if (targetAnimators != null && targetAnimators.Length > 0)
            {
                // Устанавливаем параметр для всех аниматоров в массиве
                foreach (Animator anim in targetAnimators)
                {
                    if (anim != null)
                    {
                        anim.SetBool(parameterName, true);
                        Debug.Log("Параметр " + parameterName + " установлен в true для аниматора: " + anim.gameObject.name);
                    }
                    else
                    {
                        Debug.LogWarning("Один из аниматоров в массиве не назначен!");
                    }
                }
                triggered = true;
            }
            else
            {
                Debug.LogWarning("Массив targetAnimators пуст или не назначен в инспекторе!");
            }
        }
    }
}
