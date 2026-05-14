using UnityEngine;

public class PlayerHere : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator; // Аниматор целевого объекта (не игрока)
    [SerializeField] private string parameterName = "PlayerHere";
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            if (targetAnimator != null)
            {
                targetAnimator.SetBool(parameterName, true);
                triggered = true;
                Debug.Log("Параметр " + parameterName + " установлен в true для аниматора: " + targetAnimator.gameObject.name);
            }
            else
            {
                Debug.LogWarning("targetAnimator не назначен в инспекторе!");
            }
        }
    }
}