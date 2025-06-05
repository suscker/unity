using UnityEngine;

// Атрибут CreateAssetMenu позволяет создавать именно этот ScriptableObject из меню
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("Параметры оружия")]
    public int damage;
    [HideInInspector] public int currentAmmo; // текущее количество патронов (будем считать, что выставляем при старте)
    
    [Header("Fire Settings")]
    public bool isAutomatic = false;         // Автоматический режим стрельбы
    public float fireRate = 0.1f;            // Скорострельность (выстрелов в секунду)
    public int magazineSize = 30;            // Размер магазина
    public string ammoTipe = "9mm";          // Тип патронов

    [Header("Spread Settings")]
    public float baseSpread = 1f;            // Базовый разброс (в градусах)
    public float maxSpread = 15f;             // Максимальный разброс при непрерывной стрельбе
    public float spreadIncreasePerShot = 2f;  // Увеличение разброса за выстрел
    public float spreadRecoveryRate = 5f;     // Скорость восстановления разброса (градусов/сек)

    [Header("Movement Spread Settings")]
    public float maxMovementSpread = 7f;      // Макс. доп. разброс при движении
    public float movementSpreadMultiplier = 1f; // Множитель влияния движения
}
