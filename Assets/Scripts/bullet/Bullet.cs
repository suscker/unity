using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Гарантируем наличие Rigidbody2D
public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем врага более надежным способом
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null)
        {
            // Попробуем найти в родительских объектах
            enemy = collision.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Hit enemy! Damage: {damage}");
        }
        else
        {
            Debug.Log($"Hit non-enemy: {collision.gameObject.name}");
        }

        // Всегда уничтожаем пулю при столкновении
        Destroy(gameObject);
    }
}