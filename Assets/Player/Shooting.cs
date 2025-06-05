using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    [Header("Inventory")]
    private Inventory inventory;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletForce = 70f;

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
    public float maxMovementSpread = 10f; // ������������ ���. ������� ��� ��������
    public float movementSpreadMultiplier = 1.5f; // ��������� ������� ��������
    private float currentSpread = 0f; // ������� ������� ����
    private float timeSinceLastShot = 0f; // ����� � ���������� ��������
    private PlayerMovement2D playerMovement; // ������ �� ������ ��������

    void Start()
    {
        // Get Inventory component from the same object
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Shooting: Inventory component not found on player!");
        }

        // Get PlayerMovement2D component
        playerMovement = GetComponent<PlayerMovement2D>();
        if (playerMovement == null)
        {
            Debug.LogError("Shooting: PlayerMovement2D component not found!");
        }

        // Add listener to weapon slot button
        Button weaponSlotButton = GetComponent<Button>();
        if (weaponSlotButton != null)
        {
            weaponSlotButton.onClick.AddListener(OnWeaponSlotClicked);
        }
    }

    void Update()
    {
        if (isReloading || (inventory != null && inventory.backGround != null && inventory.backGround.activeSelf))
            return;

        // ��������� ������� ������ � ��� ������
        if (inventory == null || inventory.weaponSlot == null || inventory.weaponSlot.id == 0)
            return;

        WeaponItemData weaponData = inventory.data.items[inventory.weaponSlot.id] as WeaponItemData;
        if (weaponData == null)
            return;

        // �������������� �������� �� ��������
        if (!isFiring)
        {
            timeSinceLastShot += Time.deltaTime;
            currentSpread = Mathf.Max(weaponData.baseSpread,
                                     currentSpread - weaponData.spreadRecoveryRate * Time.deltaTime);
        }

        // �������������� �������� ��� �������
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

            // ��������� �������� ��� �������
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
            // ����������� ������� �� ��������
            currentSpread = Mathf.Min(weaponData.maxSpread,
                                     currentSpread + weaponData.spreadIncreasePerShot);

            timeSinceLastShot = 0f;
            inventory.weaponSlot.currentAmmo--;

            // ������������ ����� ������� � ������ ��������
            float totalSpread = CalculateTotalSpread(weaponData);
            Shoot(weaponData, totalSpread);

            inventory.UpdateWeaponSlotText();
        }
        else
        {
            Debug.Log("Out of ammo!");
            // �������������� ����������� ��� ������ ��������
            if (!isReloading) TryReload();
        }
    }

    // ������������ ����� ������� � ������ ��������
    float CalculateTotalSpread(WeaponItemData weaponData)
    {
        // ������� ������� �� ��������
        float spread = currentSpread;

        // ��������� ������� �� ��������
        if (playerMovement != null && playerMovement.IsMoving)
        {
            // ������������ �������������� ������� � ����������� �� ��������
            float movementSpread = playerMovement.RelativeSpeed * weaponData.maxMovementSpread;
            spread += movementSpread * weaponData.movementSpreadMultiplier;
        }

        // ������������ ������������ ���������
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

        // ������������ ������� �������� ����� ��� ������ �������
        int ammoNeeded = weaponData.magazineSize - inventory.weaponSlot.currentAmmo;
        if (ammoNeeded <= 0)
        {
            Debug.Log("Magazine is already full!");
            return;
        }

        // ���� ��� ����� � ������� ���������
        List<AmmoSlotInfo> ammoSlots = FindAmmoSlots(weaponData.ammoTipe);
        if (ammoSlots.Count == 0)
        {
            Debug.Log($"No {weaponData.ammoTipe} ammo found!");
            return;
        }

        // ������������ ������� �������� �������� �����
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

        // ������� �������� �� ������� ����� �����
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

        // ����� ������� �� ������
        int remainingToTake = ammoToTake;
        foreach (var slotInfo in ammoSlots)
        {
            if (remainingToTake <= 0) break;

            // ������� ����� �� ����� �����
            int takeFromSlot = Mathf.Min(remainingToTake, slotInfo.ammoCount);

            // ��������� ���������� � �����
            inventory.items[slotInfo.slotIndex].count -= takeFromSlot;
            remainingToTake -= takeFromSlot;

            // ���� ���� �������, ������� ���
            if (inventory.items[slotInfo.slotIndex].count <= 0)
            {
                inventory.AddItem(slotInfo.slotIndex, inventory.data.items[0], 0);
            }
        }

        // �������� ������
        inventory.weaponSlot.currentAmmo += ammoToTake;
        inventory.UpdateInventory();
        Debug.Log($"Reloaded! Ammo: {inventory.weaponSlot.currentAmmo}/{weaponData.magazineSize}");
        inventory.UpdateWeaponSlotText();

        // ����� �������� ����� �����������
        currentSpread = weaponData.baseSpread;

        isReloading = false;
    }

    // ��������� ��� �������� ���������� � ������ � ���������
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

    // ����� ���� ������ � ��������� ������� ����
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

        // ������������ ��������� ����������
        float spreadAngle = Random.Range(-totalSpread, totalSpread);
        Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, spreadAngle);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(bulletRotation * Vector2.up * bulletForce, ForceMode2D.Impulse);

        // ��������� ����� ����
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = weaponData.damage;
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing Bullet component!");
        }

        // ������������ ��� �������
        Debug.DrawRay(firePoint.position, bulletRotation * Vector2.up * 5f, Color.red, 0.1f);
    }

    void OnWeaponSlotClicked()
    {
        Debug.Log("Weapon slot clicked!");
        // Implement the logic for weapon slot click
    }
}