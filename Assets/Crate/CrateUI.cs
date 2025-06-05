using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrateUI : MonoBehaviour
{
    public CrateInventory crateInventory;
    public Transform slotsContainer;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Находим инвентарь один раз перед циклом
        Inventory inventory = FindAnyObjectByType<Inventory>();

        for (int i = 0; i < slotsContainer.childCount; i++)
        {
            Transform slot = slotsContainer.GetChild(i);
            Image icon = slot.Find("Icon").GetComponent<Image>();
            TMP_Text countText = slot.Find("Count").GetComponent<TMP_Text>();

            if (i < crateInventory.items.Count)
            {
                ItemInventory item = crateInventory.items[i];

                if (item.id != 0 && item.count > 0)
                {
                    // Проверяем наличие инвентаря перед использованием
                    if (inventory != null)
                    {
                        ItemData itemData = inventory.data.items[item.id];
                        icon.sprite = itemData.img;
                        icon.enabled = true;
                        countText.text = item.count > 1 ? item.count.ToString() : "";
                    }
                    else
                    {
                        // Если инвентарь не найден, сбрасываем слот
                        icon.enabled = false;
                        countText.text = "";
                    }
                    continue;
                }
            }

            icon.enabled = false;
            countText.text = "";
        }
    }
}