using UnityEngine;
using System;

/// <summary>
/// Зона доставки предмета. Срабатывает, когда любой GameObject входит в триггер.
/// Предполагается, что в зону попадает именно предмет (например, ключ, камень и т.д.).
/// </summary>
public class DeliveryZone : MonoBehaviour
{
    [Tooltip("Вызывается, когда в зону попадает любой объект")]
    public event Action<GameObject> OnDelivered;
    private int i;
    [SerializeField] GameObject[] Books;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Book"))
        {
            Destroy(other.gameObject);
            Books[i].SetActive(true);
            i++;
            
            if (i == 3)
            {
                // other — это коллайдер предмета
                // В VR через XR Hands предметы остаются физическими объектами
                OnDelivered?.Invoke(other.gameObject);
                Destroy(gameObject);
            }
        }
    }
}