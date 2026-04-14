using UnityEngine;

public class Belt : MonoBehaviour
{
    public Transform cameraTransform;

    // Смещение (ремень ниже камеры)
    public Vector3 offset;

    void Update()
    {
        // Позиция с оффсетом
        transform.position = cameraTransform.position + offset;

        // Только поворот по Y
        float yRotation = cameraTransform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}