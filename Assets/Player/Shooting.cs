using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shooting : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletForce = 20f;

    [Header("Reload Settings")]
    public AudioClip reloadSound;
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Fire Sound")]
    public AudioClip fireSound;

    [Header("Automatic Fire Settings")]
    private float nextFireTime = 0f;
    private bool isFiring = false;

    [Header("Spread Settings")]
    public float maxMovementSpread = 10f; // Максимальный доп. разброс при движении
    public float movementSpreadMultiplier = 1.5f; // Множитель влияния движения
    private float currentSpread = 0f; // Текущий разброс пуль
    private float timeSinceLastShot = 0f; // Время с последнего выстрела
    private PlayerMovement2D playerMovement; // Ссылка на скрипт движения

    void Start()
    {
        // Получаем компонент движения игрока
        playerMovement = GetComponent<PlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("Shooting: PlayerMovement2D component not found!");
        }
    }

    void Update()
    {
        if (isReloading || (inventory != null && inventory.backGround != null && inventory.backGround.activeSelf))
            return;

        // Проверяем наличие оружия и его данных
        if (inventory == null || inventory.weaponSlot == null || inventory.weaponSlot.id == 0)
            return;

        WeaponItemData weaponData = inventory.data.items[inventory.weaponSlot.id] as WeaponItemData;
        if (weaponData == null)
            return;

        // Восстановление разброса со временем
        if (!isFiring)
        {
            timeSinceLastShot += Time.deltaTime;
            currentSpread = Mathf.Max(weaponData.baseSpread,
                                     currentSpread - weaponData.spreadRecoveryRate * Time.deltaTime);
        }

        // Автоматическая стрельба при зажатии
        if (Input.GetButton("Fire1") && weaponData.isAutomatic)
        {
            isFiring = true;
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + weaponData.fireRate;
                TryShoot(weaponData);
            }
        }
        else
        {
            isFiring = false;

            // Одиночная стрельба при нажатии
            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot(weaponData);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }

    void TryShoot(WeaponItemData weaponData)
    {
        if (inventory.weaponSlot.currentAmmo > 0)
        {
            // Увеличиваем разброс от стрельбы
            currentSpread = Mathf.Min(weaponData.maxSpread,
                                     currentSpread + weaponData.spreadIncreasePerShot);

            timeSinceLastShot = 0f;
            inventory.weaponSlot.currentAmmo--;

            // Рассчитываем общий разброс с учетом движения
            float totalSpread = CalculateTotalSpread(weaponData);
            Shoot(weaponData, totalSpread);

            inventory.UpdateWeaponSlotText();
        }
        else
        {
            Debug.Log("Out of ammo!");
            // Автоматическая перезарядка при пустом магазине
            if (!isReloading) TryReload();
        }
    }

    // Рассчитывает общий разброс с учетом движения
    float CalculateTotalSpread(WeaponItemData weaponData)
    {
        // Базовый разброс от стрельбы
        float spread = currentSpread;

        // Добавляем разброс от движения
        if (playerMovement != null && playerMovement.IsMoving)
        {
            // Рассчитываем дополнительный разброс в зависимости от скорости
            float movementSpread = playerMovement.RelativeSpeed * weaponData.maxMovementSpread;
            spread += movementSpread * weaponData.movementSpreadMultiplier;
        }

        // Ограничиваем максимальным значением
        return Mathf.Min(spread, weaponData.maxSpread + weaponData.maxMovementSpread);
    }

    void TryReload()
    {
        if (inventory == null || inventory.weaponSlot == null || inventory.weaponSlot.id == 0)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        WeaponItemData weaponData = inventory.data.items[inventory.weaponSlot.id] as WeaponItemData;
        if (weaponData == null)
        {
            Debug.Log("Invalid weapon data!");
            return;
        }

        // Рассчитываем сколько патронов нужно для полной зарядки
        int ammoNeeded = weaponData.magazineSize - inventory.weaponSlot.currentAmmo;
        if (ammoNeeded <= 0)
        {
            Debug.Log("Magazine is already full!");
            return;
        }

        // Ищем все слоты с нужными патронами
        List<AmmoSlotInfo> ammoSlots = FindAmmoSlots(weaponData.ammoTipe);
        if (ammoSlots.Count == 0)
        {
            Debug.Log($"No {weaponData.ammoTipe} ammo found!");
            return;
        }

        // Рассчитываем сколько патронов доступно всего
        int totalAmmoAvailable = 0;
        foreach (var slot in ammoSlots)
        {
            totalAmmoAvailable += slot.ammoCount;
        }

        if (totalAmmoAvailable == 0)
        {
            Debug.Log($"No {weaponData.ammoTipe} ammo available!");
            return;
        }

        // Сколько патронов мы реально можем взять
        int ammoToTake = Mathf.Min(ammoNeeded, totalAmmoAvailable);
        StartCoroutine(Reload(ammoSlots, ammoToTake, weaponData));
    }

    IEnumerator Reload(List<AmmoSlotInfo> ammoSlots, int ammoToTake, WeaponItemData weaponData)
    {
        isReloading = true;
        Debug.Log($"Reloading {ammoToTake} rounds...");

        if (reloadSound != null)
            AudioSource.PlayClipAtPoint(reloadSound, transform.position);

        yield return new WaitForSeconds(reloadTime);

        // Берем патроны из слотов
        int remainingToTake = ammoToTake;
        foreach (var slotInfo in ammoSlots)
        {
            if (remainingToTake <= 0) break;

            // Сколько взять из этого слота
            int takeFromSlot = Mathf.Min(remainingToTake, slotInfo.ammoCount);

            // Уменьшаем количество в слоте
            inventory.items[slotInfo.slotIndex].count -= takeFromSlot;
            remainingToTake -= takeFromSlot;

            // Если слот опустел, очищаем его
            if (inventory.items[slotInfo.slotIndex].count <= 0)
            {
                inventory.AddItem(slotInfo.slotIndex, inventory.data.items[0], 0);
            }
        }

        // Заряжаем оружие
        inventory.weaponSlot.currentAmmo += ammoToTake;
        inventory.UpdateInventory();
        Debug.Log($"Reloaded! Ammo: {inventory.weaponSlot.currentAmmo}/{weaponData.magazineSize}");
        inventory.UpdateWeaponSlotText();

        // Сброс разброса после перезарядки
        currentSpread = weaponData.baseSpread;

        isReloading = false;
    }

    // Структура для хранения информации о слотах с патронами
    private struct AmmoSlotInfo
    {
        public int slotIndex;
        public int ammoCount;

        public AmmoSlotInfo(int index, int count)
        {
            slotIndex = index;
            ammoCount = count;
        }
    }

    // Поиск всех слотов с патронами нужного типа
    List<AmmoSlotInfo> FindAmmoSlots(string ammoType)
    {
        List<AmmoSlotInfo> slots = new List<AmmoSlotInfo>();

        for (int i = 0; i < inventory.maxCount; i++)
        {
            if (inventory.items[i].id == 0) continue;

            ItemData item = inventory.data.items[inventory.items[i].id];

            if (item is ItemLoot && item.itemName == ammoType)
            {
                slots.Add(new AmmoSlotInfo(i, inventory.items[i].count));
            }
        }
        return slots;
    }

    void Shoot(WeaponItemData weaponData, float totalSpread)
    {
        if (fireSound != null)
            AudioSource.PlayClipAtPoint(fireSound, transform.position);

        // Рассчитываем случайное отклонение
        float spreadAngle = Random.Range(-totalSpread, totalSpread);
        Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, spreadAngle);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(bulletRotation * Vector2.up * bulletForce, ForceMode2D.Impulse);

        // Установка урона пуле
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = weaponData.damage;
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing Bullet component!");
        }

        // Визуализация для отладки
        Debug.DrawRay(firePoint.position, bulletRotation * Vector2.up * 5f, Color.red, 0.1f);
    }
}