using UnityEngine;

public enum AmmoType
{
    sniperRifle,
    automaticRifle,
    // ... при желании добавьте другие типы патронов
}

// Атрибут CreateAssetMenu позволяет создавать именно этот ScriptableObject из меню
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("Параметры оружия")]
    public int damage;
    public int magazineSize;
    [HideInInspector] public int currentAmmo; // текущее количество патронов (будем считать, что выставляем при старте)
    public float fireRate;
    public AmmoType ammoType;

    public override void Use()
    {
        Debug.Log($"Firing weapon: {itemName}");
        // здесь можно вызывать логику стрельбы, но обычно мы не реализуем её прямо тут,
        // а делегируем в WeaponSystem (см. далее).
    }
}
