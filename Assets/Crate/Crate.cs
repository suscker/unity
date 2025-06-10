using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Basic Settings")]
    public GameObject inventoryUI; // UI панель инвентаря ящика
    public int maxSlots = 20;      // Максимальное количество слотов
    private List<ItemInventory> items = new List<ItemInventory>();
    private bool isOpen = false;
    [Header("Loot Spawn Rate")]
    public float lootSpawnRate = 0.25f;

    [Header("References")]
    public ItemDatabase itemDatabase; // Ссылка на базу данных предметов
    public GameObject slotPrefab; // Префаб ячейки
    public Transform slotsParent; // Контейнер для ячеек

    private List<GameObject> slotInstances = new List<GameObject>();

    private void Start()
    {
        // Инициализируем инвентарь
        InitializeInventory();

        // Регистрируем ящик в SaveManager
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterCrate(this);
        }
    }

    private void InitializeInventory()
    {
        items.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(new ItemInventory { id = 0, count = 0 });
        }
    }
    public void Interact(Inventory playerInventory)
    {
        if (!isOpen)
        {
            // Открываем ящик
            OpenCrate(playerInventory);
        }
        else
        {
            // Закрываем ящик
            CloseCrate(playerInventory);
        }
    }
    public void OpenCrate(Inventory playerInventory)
    {
        isOpen = true;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
        if (items.TrueForAll(item => item.id == 0))
        {
            GenerateLoot();
        }
        playerInventory.OpenInventoryWithCrate(inventoryUI);
        var crateUI = inventoryUI != null ? inventoryUI.GetComponentInChildren<CrateInventoryUI>() : null;
        if (crateUI != null)
        {
            // Получаем созданные слоты
            var slotObjects = crateUI.GetSlotObjects();
            
            // Связываем слоты с items
            for (int i = 0; i < items.Count; i++)
            {
                items[i].itemGameObj = slotObjects[i];
            }
            
            StartCoroutine(DelayedUpdateUI(crateUI, items));
        }
    }

    private System.Collections.IEnumerator DelayedUpdateUI(CrateInventoryUI crateUI, List<ItemInventory> items)
    {
        yield return null; // Ждём 1 кадр, чтобы Canvas успел активироваться
        crateUI.UpdateUI(items);
    }

    public void CloseCrate(Inventory playerInventory)
    {
        isOpen = false;
        // Скрываем UI инвентаря
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        // Закрываем инвентарь игрока
        playerInventory.CloseInventory();
    }

    // Получить список предметов в ящике
    public List<ItemInventory> GetItems()
    {
        return items;
    }

    // Установить список предметов в ящике
    public void SetItems(List<ItemInventory> newItems)
    {
        items = newItems;
    }

    private void GenerateLoot()
    {
        Debug.Log("Генерирую лут для ящика!");
        // Проверяем наличие ссылки на базу данных
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase reference is not set in the Crate component!");
            return;
        }

        // Проходим по всем слотам
        for (int i = 0; i < maxSlots; i++)
        {
            // шанс генерации предмета
            if (Random.value <= lootSpawnRate)
            {
                // Выбираем случайный предмет из базы данных (кроме id = 0)
                int randomIndex;
                randomIndex = Random.Range(1, itemDatabase.items.Count);

                ItemData randomItem = itemDatabase.items[randomIndex];
                
                // Определяем количество предметов
                int count = 1;
                if (randomItem.maxCountInStack > 1)
                {
                    count = Random.Range(1, randomItem.maxCountInStack + 1);
                }

                if (randomItem is WeaponItemData weaponData)
                {
                    items[i] = new WeaponInventory(
                        new ItemInventory { id = weaponData.id, count = count },
                        weaponData
                    );
                }
                else if (randomItem is ItemArmor armorData)
                {
                    items[i] = new ArmorInventory(
                        new ItemInventory { id = armorData.id, count = count },
                        armorData
                    );
                }
                else if (randomItem is ItemHeal healData)
                {
                    items[i] = new HealInventory(
                        new ItemInventory { id = healData.id, count = count },
                        healData
                    );
                }
                else
                {
                    items[i] = new ItemInventory { id = randomItem.id, count = count };
                }
            }
            else
            {
                items[i] = new ItemInventory { id = 0, count = 0 };
            }
            // Если шанс не выпал, слот остается пустым (id = 0)
        }
        Debug.Log("Содержимое ящика после генерации:");
        foreach (var item in items)
            Debug.Log($"id={item.id}, count={item.count}");
    }
    public void UpdateInventory()
    {
        Debug.Log("UpdateInventory");
        for (int i = 0; i < maxSlots; i++)
        {
            var go = items[i].itemGameObj;
            if(go == null)
            {
                Debug.LogError("go == null");
                continue;
            }
            if(go.GetComponent<Image>() == null)
            {
                Debug.LogError("go.GetComponent<Image>()==null");
                continue;
            }
            var imgComp = go.GetComponent<Image>();
            var txtComp = go.GetComponentInChildren<TMP_Text>();

            if (imgComp != null)
                imgComp.sprite = itemDatabase.items[items[i].id].img;

            if (items[i].id != 0 && items[i].count > 0)
            {
                if (txtComp != null)
                {
                    if (items[i] is WeaponInventory weaponSlot)
                        txtComp.text = $"{weaponSlot.currentAmmo}/{weaponSlot.magazineSize}";
                    else if (items[i] is ArmorInventory armorSlot)
                        txtComp.text = $"{armorSlot.currentDurability}/{armorSlot.maxDurability}";
                    else if (items[i] is HealInventory healSlot)
                        txtComp.text = $"{healSlot.currentHeal}/{healSlot.maxHeal}";
                    else
                        txtComp.text = items[i].count > 1 ? items[i].count.ToString() : "";
                }
            }
            else
            {
                if (txtComp != null)
                    txtComp.text = "";
            }
        }
    }

    public ItemInventory GetItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return null;
        
        ItemInventory item = items[slotIndex];
        items[slotIndex] = new ItemInventory { id = 0, count = 0 };
        
        // Обновляем UI
        var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
        if (crateUI != null)
        {
            crateUI.UpdateUI(items);
        }
        
        return item;
    }
    public (bool success, ItemInventory remainingItems) PutItemInSlot(int slotIndex, ItemInventory item)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) 
            return (false, item);
        
        // Если слот пустой
        if (items[slotIndex].id == 0)
        {
            items[slotIndex] = item;
            var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
            if (crateUI != null)
            {
                crateUI.UpdateUI(items);
            }
            //UpdateSlotUI(slotIndex);
            return (true, null); // Успешно положили весь стак
        }
        // Если в слоте такой же предмет и можно стакать
        else if (items[slotIndex].id == item.id)
        {
            int maxStack = itemDatabase.items[item.id].maxCountInStack;
            int totalCount = items[slotIndex].count + item.count;
            
            if (totalCount <= maxStack)
            {
                items[slotIndex].count = totalCount;
                //UpdateSlotUI(slotIndex);
                var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
                if (crateUI != null)
                {
                    crateUI.UpdateUI(items);
                }
                return (true, null); // Успешно положили весь стак
            }
            else
            {
                // Заполняем стак до максимума
                int remainingCount = totalCount - maxStack;
                items[slotIndex].count = maxStack;
                var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
                if (crateUI != null)
                {
                    crateUI.UpdateUI(items);
                }
                //UpdateSlotUI(slotIndex);
                // Возвращаем оставшиеся предметы
                ItemInventory remaining = new ItemInventory 
                { 
                    id = item.id, 
                    count = remainingCount 
                };
                return (true, remaining);
            }
        }
        else
        {
            var temp = items[slotIndex];
            items[slotIndex] = item;
            // Обновляем UI только для этого слота
            var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
            if (crateUI != null)
            {
                crateUI.UpdateUI(items);
            }
            // Возвращаем старый предмет как "оставшийся"
            return (true, temp);
        }
    }
    /*
    public void UpdateSlotUI(int slotIndex)
    {
        if (slotObjects == null || slotIndex < 0 || slotIndex >= slotObjects.Count)
            return;

        GameObject slot = slotObjects[slotIndex];
        // Получаем предмет из списка
        var item = crateReference.GetItems()[slotIndex]; // crateReference — ссылка на Crate, или передайте список в CrateInventoryUI

        Image imgComp = slot.GetComponent<Image>();
        TMP_Text txtComp = slot.GetComponentInChildren<TMP_Text>();

        if (imgComp != null && itemDatabase != null)
            imgComp.sprite = itemDatabase.items[item.id].img;

        if (item.id != 0 && item.count > 0)
        {
            if (item is WeaponInventory weaponSlot)
                txtComp.text = $"{weaponSlot.currentAmmo}/{weaponSlot.magazineSize}";
            else if (item is ArmorInventory armorSlot)
                txtComp.text = $"{armorSlot.currentDurability}/{armorSlot.maxDurability}";
            else if (item is HealInventory healSlot)
                txtComp.text = $"{healSlot.currentHeal}/{healSlot.maxHeal}";
            else
                txtComp.text = item.count > 1 ? item.count.ToString() : "";
        }
        else
        {
            if (txtComp != null)
                txtComp.text = "";
        }
    }
    */

    private void OnDestroy()
    {
        // Отписываемся от SaveManager при уничтожении
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnregisterCrate(this);
        }
    }

    public InventorySaveData GetInventoryData()
    {
        var saveData = new InventorySaveData();
        
        // Сохраняем предметы
        foreach (var item in items)
        {
            var itemSave = new InventorySaveData.ItemSaveData
            {
                id = item.id,
                count = item.count
            };

            if (item is WeaponInventory weapon)
            {
                itemSave.itemType = "Weapon";
                itemSave.currentAmmo = weapon.currentAmmo;
                itemSave.magazineSize = weapon.magazineSize;
            }
            else if (item is ArmorInventory armor)
            {
                itemSave.itemType = "Armor";
                itemSave.currentDurability = armor.currentDurability;
                itemSave.maxDurability = armor.maxDurability;
            }
            else if (item is HealInventory heal)
            {
                itemSave.itemType = "Heal";
                itemSave.currentHeal = heal.currentHeal;
                itemSave.maxHeal = heal.maxHeal;
            }
            else
            {
                itemSave.itemType = "Item";
            }

            saveData.items.Add(itemSave);
        }

        return saveData;
    }

    public void LoadFromSaveData(InventorySaveData saveData)
    {
        if (saveData == null) return;

        // Очищаем текущий инвентарь
        InitializeInventory();

        // Загружаем предметы
        for (int i = 0; i < saveData.items.Count && i < maxSlots; i++)
        {
            var itemSave = saveData.items[i];
            if (itemSave.id == 0) continue;

            switch (itemSave.itemType)
            {
                case "Weapon":
                    var weaponData = itemDatabase.items[itemSave.id] as WeaponItemData;
                    if (weaponData != null)
                    {
                        var weaponSlot = new WeaponInventory(
                            new ItemInventory { id = itemSave.id, count = itemSave.count },
                            weaponData
                        )
                        {
                            currentAmmo = itemSave.currentAmmo,
                            magazineSize = itemSave.magazineSize
                        };
                        items[i] = weaponSlot;
                    }
                    break;

                case "Armor":
                    var armorData = itemDatabase.items[itemSave.id] as ItemArmor;
                    if (armorData != null)
                    {
                        var armorSlot = new ArmorInventory(
                            new ItemInventory { id = itemSave.id, count = itemSave.count },
                            armorData
                        )
                        {
                            currentDurability = itemSave.currentDurability,
                            maxDurability = itemSave.maxDurability
                        };
                        items[i] = armorSlot;
                    }
                    break;

                case "Heal":
                    var healData = itemDatabase.items[itemSave.id] as ItemHeal;
                    if (healData != null)
                    {
                        var healSlot = new HealInventory(
                            new ItemInventory { id = itemSave.id, count = itemSave.count },
                            healData
                        )
                        {
                            currentHeal = itemSave.currentHeal,
                            maxHeal = itemSave.maxHeal
                        };
                        items[i] = healSlot;
                    }
                    break;

                default:
                    items[i] = new ItemInventory { id = itemSave.id, count = itemSave.count };
                    break;
            }
        }

        // Обновляем UI если ящик открыт
        if (isOpen)
        {
            var crateUI = inventoryUI.GetComponentInChildren<CrateInventoryUI>();
            if (crateUI != null)
            {
                crateUI.UpdateUI(items);
            }
        }
    }
}