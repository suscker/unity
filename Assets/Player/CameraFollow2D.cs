using UnityEngine;

/// <summary>
/// Простой скрипт, чтобы камера следовала за целевым объектом (Player) с небольшим сглаживанием.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Tooltip("Цель, за которой должна следовать камера (обычно Transform игрока)")]
    public Transform target;

    [Tooltip("Сглаживание движения камеры (большее значение = более плавная, медленная подстройка)")]
    public float smoothTime = 0.2f;

    // Смещение камеры относительно игрока, если нужно (например, чтобы камера чуть-чуть смотрела вперёд)
    [Tooltip("Сдвиг камеры относительно позиции игрока")]
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Желаемая позиция камеры
        Vector3 targetPosition = target.position + offset;

        // Плавно двигаем камеру из текущей позиции в targetPosition
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
