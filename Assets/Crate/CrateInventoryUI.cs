using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CrateInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject slotPrefab;        // Префаб слота
    public Transform slotsParent;        // Родительский объект для слотов
    public int maxSlots = 20;           // Максимальное количество слотов
    public ItemDatabase itemDatabase;

    private List<GameObject> slotObjects = new List<GameObject>();

    private void Start()
    {
        InitializeSlots();
    }

    public List<GameObject> GetSlotObjects()
    {
        return slotObjects;
    }

    private void InitializeSlots()
    {
        // Очищаем существующие слоты
        foreach (var slot in slotObjects)
        {
            if (slot != null)
                Destroy(slot);
        }
        slotObjects.Clear();

        // Создаем новые слоты
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            slot.name = $"Slot_{i}";
            slotObjects.Add(slot);
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    // Получаем ссылку на Inventory (например, через синглтон или FindObjectOfType)
                    var inventory = FindAnyObjectByType<Inventory>();
                    if (inventory != null)
                        inventory.SelectObject();
                });
            }
        }
    }

    public void UpdateUI(List<ItemInventory> items)
    {
        if (items == null || slotObjects.Count != items.Count)
        {
            Debug.LogError("Items list is null or count doesn't match!");
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            GameObject slot = slotObjects[i];
            ItemInventory item = items[i];

            Image imgComp = slot.GetComponent<Image>();
            TMP_Text txtComp = slot.GetComponentInChildren<TMP_Text>();

            // Устанавливаем спрайт даже для id == 0!
            if (imgComp != null && itemDatabase != null)
                imgComp.sprite = itemDatabase.items[item.id].img;

            // Устанавливаем текст
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
    }

    public int GetSlotCount() => slotObjects.Count;

    public void EnsureSlots()
    {
        if (slotObjects.Count != maxSlots)
            InitializeSlots();
    }

    private void OnEnable()
    {
        Debug.Log("CrateInventoryUI.OnEnable вызван!");
        if (slotObjects.Count != maxSlots)
            InitializeSlots();
    }
} 