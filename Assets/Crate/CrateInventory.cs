using UnityEngine;
using System.Collections.Generic;

public class CrateInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<ItemInventory> items = new List<ItemInventory>();
    public int maxSlots = 20;
    public bool isOpen = false;

    [Header("UI Reference")]
    public GameObject inventoryUI;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 2f;

    private Transform player;
    private Inventory playerInventory;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerInventory = player.GetComponent<Inventory>();


        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        if (inventoryUI != null)
        {
            CrateUI crateUI = inventoryUI.GetComponent<CrateUI>();
            if (crateUI != null && crateUI.slotsContainer != null)
            {
                // Очистка старых слотов
                foreach (Transform child in crateUI.slotsContainer)
                    Destroy(child.gameObject);

                // Создание новых слотов
                for (int i = 0; i < maxSlots; i++)
                {
                    GameObject slot = Instantiate(Resources.Load<GameObject>("Slot"));
                    slot.transform.SetParent(crateUI.slotsContainer);
                    slot.name = i.ToString();
                }
            }
        }

        // Инициализация пустых слотов
        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(new ItemInventory { id = 0, count = 0 });
        }
        AddItem(FindObjectOfType<Inventory>().data.items[1], 5); // Пример предмета
        UpdateUI();

    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
            return;
        }

        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= interactRange)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (isOpen)
                {
                    CloseInventory();
                }
                else
                {
                    OpenInventory();
                }
            }
        }
        else if (isOpen)
        {
            CloseInventory();
        }
    }

    public void OpenInventory()
    {
        isOpen = true;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            UpdateUI();
        }

        // Блокируем управление игроком
        PlayerMovement2D playerMovement = player.GetComponent<PlayerMovement2D>();
        if (playerMovement != null)
            playerMovement.SetCanMove(false);

        // Открываем инвентарь игрока вместе с ящиком
        if (playerInventory != null)
        {
            playerInventory.OpenInventoryWithCrate(inventoryUI);
        }
    }

    public void CloseInventory()
    {
        isOpen = false;

        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        // Закрываем инвентарь игрока
        if (playerInventory != null)
            playerInventory.CloseInventory();

        // Восстанавливаем управление игроком
        PlayerMovement2D playerMovement = player.GetComponent<PlayerMovement2D>();
        if (playerMovement != null)
            playerMovement.SetCanMove(true);
    }

    public bool AddItem(ItemData item, int count = 1)
    {
        // Поиск существующего стека
        foreach (ItemInventory slot in items)
        {
            if (slot.id == item.id && slot.count < item.maxCountInStack)
            {
                int space = item.maxCountInStack - slot.count;
                int addAmount = Mathf.Min(space, count);
                slot.count += addAmount;
                count -= addAmount;

                if (count <= 0)
                {
                    UpdateUI();
                    return true;
                }
            }
        }

        // Поиск пустого слота
        foreach (ItemInventory slot in items)
        {
            if (slot.id == 0)
            {
                slot.id = item.id;
                slot.count = count;
                UpdateUI();
                return true;
            }
        }

        return false; // Нет места
    }

    public void RemoveItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= items.Count)
            return;

        items[slotIndex].count -= count;
        if (items[slotIndex].count <= 0)
        {
            items[slotIndex] = new ItemInventory { id = 0, count = 0 };
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Здесь должна быть реализация обновления UI
        // Например: if (inventoryUI != null) inventoryUI.GetComponent<CrateUI>().UpdateUI(items);
        Debug.Log("Update UI called");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}