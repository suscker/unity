using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Armor Settings")]
    public bool hasArmor = false;
    public int armorEfficiency = 0;
    public int armorDurability = 0;

    public void TakeDamage(int damage)
    {
        if (hasArmor)
        {
            int absorbedDamage = Mathf.RoundToInt(damage * armorEfficiency / 100f);
            int finalDamage = damage - absorbedDamage;

            armorDurability -= absorbedDamage;
            if (armorDurability <= 0)
            {
                hasArmor = false;
                armorEfficiency = 0;
                // Здесь можно вызвать событие слома брони
            }

            currentHealth -= finalDamage;
            Debug.Log($"Armor absorbed: {absorbedDamage} | Health damage: {finalDamage}");
        }
        else
        {
            currentHealth -= damage;
        }

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Healed: +{amount} HP");
    }

    public void EquipArmor(ItemArmor armor)
    {
        hasArmor = true;
        armorEfficiency = armor.efficiency;
        armorDurability = armor.currentHP;
    }

    public void RemoveArmor()
    {
        hasArmor = false;
        armorEfficiency = 0;
        armorDurability = 0;
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Реализация смерти
    }
}