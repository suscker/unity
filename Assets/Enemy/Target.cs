using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isInvincible = false; // Для тренировочных мишеней

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Enemy get {damage} damage");
        // Мигание красным цветом
        StartCoroutine(DamageEffect());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageEffect()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color original = renderer.color;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.01f);
            renderer.color = original;
        }
    }

    void Die()
    {
        Debug.Log($"{name} destroyed!");
        Destroy(gameObject);
    }
}