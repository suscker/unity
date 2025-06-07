using UnityEngine;
using System.Collections.Generic;

public class Crate : MonoBehaviour, IInteractable
{
    [Header("Basic Settings")]
    public GameObject inventoryUI; // UI панель инвентаря ящика
    public int maxSlots = 20;      // Максимальное количество слотов
    private List<ItemInventory> items = new List<ItemInventory>();
    private bool isOpen = false;

    [Header("References")]
    public ItemDatabase itemDatabase; // Ссылка на базу данных предметов
    public GameObject slotPrefab; // Префаб ячейки
    public Transform slotsParent; // Контейнер для ячеек

    private List<GameObject> slotInstances = new List<GameObject>();

    private void Start()
    {
        // Инициализируем инвентарь
        InitializeInventory();
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
            // 25% шанс генерации предмета
            if (Random.value <= 0.25f)
            {
                // Выбираем случайный предмет из базы данных (кроме id = 0)
                int randomIndex;
                do
                {
                    randomIndex = Random.Range(0, itemDatabase.items.Count);
                } while (itemDatabase.items[randomIndex].id == 0);

                ItemData randomItem = itemDatabase.items[randomIndex];
                
                // Определяем количество предметов
                int count = 1;
                if (randomItem.maxCountInStack > 1)
                {
                    count = Random.Range(1, randomItem.maxCountInStack + 1);
                }

                // Создаем предмет в слоте
                items[i] = new ItemInventory
                {
                    id = randomItem.id,
                    count = count
                };
            }
            else
            {
                items[i] = new ItemInventory { id = 0, count = 0 };
            }
            // Если шанс не выпал, слот остается пустым (id = 0)
        }
        /*
        Debug.Log("Содержимое ящика после генерации:");
        foreach (var item in items)
            Debug.Log($"id={item.id}, count={item.count}");
        */
    }
}