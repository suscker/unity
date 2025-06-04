using UnityEngine;
using TMPro; 

public class HealthUI : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Сюда перетащите объект с PlayerHealth")]
    public PlayerHealth playerHealth;

    [Tooltip("Сюда перетащите TextMeshProUGUI (или Text) из Canvas")]
    public TMP_Text healthText; // если Text, то public Text healthText;

    private void Start()
    {
        if (playerHealth == null)
            Debug.LogError("HealthUI: не назначен PlayerHealth в инспекторе");

        if (healthText == null)
            Debug.LogError("HealthUI: не назначен healthText (TMP_Text) в инспекторе");

        UpdateHealthDisplay();
    }

    private void Update()
    {
        // Каждый кадр проверяем, изменилось ли здоровье, и пересчитываем строку
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (playerHealth == null || healthText == null)
            return;

        // Получаем текущее и максимальное здоровье
        int cur = playerHealth.CurrentHealth;
        int max = playerHealth.maxHealth; // сделайте поле maxHealth публичным или создайте свойство

        // Формируем строку: например, "HP: 75/100"
        healthText.text = $"HP: {cur}/{max}";
    }
}
