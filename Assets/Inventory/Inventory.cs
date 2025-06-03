using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public DataBase data;
    public List<ItemInventory> items = new List<ItemInventory>();

    public GameObject gameObjShow;

    public GameObject InventoryMainObject;

    public int maxCount;

    public Camera cam;
    public EventSystem es;

    public int currentID;
    public ItemInventory currentItem;

    public RectTransform movingObject;
    public Vector3 offset;

    public GameObject backGround;

    public ItemInventory weaponSlot = new ItemInventory();
    public GameObject weaponSlotObject; 
    public Sprite weaponSlotDefaultSprite;

    public ItemInventory armorSlot = new ItemInventory();
    public GameObject armorSlotObject;
    public Sprite armorSlotDefaultSprite;

    public ItemInventory healSlot = new ItemInventory();
    public GameObject healSlotObject;
    public Sprite healSlotDefaultSprite;


    public void Start()
    {
        if (items.Count == 0)
        {
            AddGraphics();
        }
        weaponSlot.itemGameObj = weaponSlotObject;
        weaponSlot.id = 0;
        armorSlot.itemGameObj = armorSlotObject;
        armorSlot.id = 0;
        healSlot.itemGameObj = healSlotObject;
        healSlot.id = 0;
        /*
         * это 
         * для
         * теста
        */
        ///Debug.Log("Ya tyt");
        for (int i = 0; i < maxCount; i++)
        {
            int number = Random.Range(0, data.items.Count);
            AddItem(i, data.items[number], Random.Range(1, data.items[number].maxCountInStack));
        }

    }

    bool IsHeal(Item item)
    {
        if (item.type == ItemType.Heal) return true;
        return false;
    }

    public void EquipHeal(Item item)
    {
        if (item == null || item.id == 0)
            return;

        healSlot.id = item.id;
        healSlot.count = 1;
        healSlot.itemGameObj.GetComponent<Image>().sprite = item.img;

        var text = healSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = item.name; // или просто ""
    }

    public void OnHealSlotClicked()
    {
        if (currentID == -1)
        {
            // В руке ничего — взять из healSlot
            if (healSlot.id == 0 || healSlot.count == 0) return;

            currentItem = CopyInventoryItem(healSlot);

            healSlot.id = 0;
            healSlot.count = 0;
            healSlot.itemGameObj.GetComponent<Image>().sprite = healSlotDefaultSprite;
            healSlot.itemGameObj.GetComponent<Image>().color = Color.white;
            UpdateHealSlotText();

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;

            currentID = -2; // -2 означает "взято из оружейного слота"
        }
        else
        {
            // Есть предмет в руке
            if (!IsHeal(data.items[currentItem.id])) return;

            if (healSlot.id != 0 && healSlot.count > 0)
            {
                int remaining = SearchForSameItem(data.items[healSlot.id], healSlot.count);
                if (remaining > 0)
                {
                    // Если не удалось уместить всё — можно создать новый слот или сообщить игроку
                    Debug.LogWarning("Не удалось полностью вернуть предмет из Heal-слота в инвентарь. Осталось: " + remaining);
                }
            }

            // Экипировать предмет из руки в слот
            healSlot.id = currentItem.id;
            healSlot.count = currentItem.count;
            healSlot.itemGameObj.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateHealSlotText();

            // Очистить курсор
            currentItem = new ItemInventory();
            movingObject.gameObject.SetActive(false);
            currentID = -1;
        }
    }

    void UpdateHealSlotText()
    {
        var text = healSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = healSlot.id == 0 ? "" : data.items[healSlot.id].name;
    } 


    ///
    bool IsArmor(Item item)
    {
        if (item.type == ItemType.Armor) return true;
        return false;
    }

    public void EquipArmor(Item item)
    {
        if (item == null || item.id == 0)
            return;

        armorSlot.id = item.id;
        armorSlot.count = 1;
        armorSlot.itemGameObj.GetComponent<Image>().sprite = item.img;

        var text = armorSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = item.name; // или просто ""
    }

    public void OnArmorSlotClicked()
    {
        if (currentID == -1)
        {
            // В руке ничего — взять из armorSlot
            if (armorSlot.id == 0 || armorSlot.count == 0) return;

            currentItem = CopyInventoryItem(armorSlot);

            armorSlot.id = 0;
            armorSlot.count = 0;
            armorSlot.itemGameObj.GetComponent<Image>().sprite = armorSlotDefaultSprite;
            armorSlot.itemGameObj.GetComponent<Image>().color = Color.white;
            UpdateArmorSlotText();

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;

            currentID = -2; // -2 означает "взято из оружейного слота"
        }
        else
        {
            // Есть предмет в руке
            if (!IsArmor(data.items[currentItem.id])) return;

            if (armorSlot.id != 0 && armorSlot.count > 0)
            {
                int remaining = SearchForSameItem(data.items[armorSlot.id], armorSlot.count);
                if (remaining > 0)
                {
                    Debug.LogWarning("Не удалось полностью вернуть предмет из Armor-слота в инвентарь. Осталось: " + remaining);
                }
            }

            // Экипировать предмет из руки в слот
            armorSlot.id = currentItem.id;
            armorSlot.count = currentItem.count;
            armorSlot.itemGameObj.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateArmorSlotText();

            // Очистить курсор
            currentItem = new ItemInventory();
            movingObject.gameObject.SetActive(false);
            currentID = -1;
        }
    }

    void UpdateArmorSlotText()
    {
        var text = armorSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = armorSlot.id == 0 ? "" : data.items[armorSlot.id].name;
    }

    ///

    bool IsWeapon(Item item)
    {
        if (item.type == ItemType.Weapon) return true;
        return false;
    }

    public void EquipWeapon(Item item)
    {
        if (item == null || item.id == 0)
            return;

        weaponSlot.id = item.id;
        weaponSlot.count = 1;
        weaponSlot.itemGameObj.GetComponent<Image>().sprite = item.img;

        var text = weaponSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = item.name; // или просто ""
    }

    public void OnWeaponSlotClicked()
    {
        if (currentID == -1)
        {
            // В руке ничего — взять из weaponSlot
            if (weaponSlot.id == 0 || weaponSlot.count == 0) return;

            currentItem = CopyInventoryItem(weaponSlot);

            weaponSlot.id = 0;
            weaponSlot.count = 0;
            weaponSlot.itemGameObj.GetComponent<Image>().sprite = weaponSlotDefaultSprite;
            weaponSlot.itemGameObj.GetComponent<Image>().color = Color.white;
            UpdateWeaponSlotText();

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;

            currentID = -2; // -2 означает "взято из оружейного слота"
        }
        else
        {
            // Есть предмет в руке
            if (!IsWeapon(data.items[currentItem.id])) return;

            // Если в слоте уже есть предмет — вернуть его в инвентарь
            if (weaponSlot.id != 0 && weaponSlot.count > 0)
            {
                int remaining = SearchForSameItem(data.items[weaponSlot.id], weaponSlot.count);
                if (remaining > 0)
                {
                    Debug.LogWarning("Не удалось полностью вернуть предмет из Weapon-слота в инвентарь. Осталось: " + remaining);
                }
            }

            // Экипировать предмет из руки в слот
            weaponSlot.id = currentItem.id;
            weaponSlot.count = currentItem.count;
            weaponSlot.itemGameObj.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateWeaponSlotText();

            // Очистить курсор
            currentItem = new ItemInventory();
            movingObject.gameObject.SetActive(false);
            currentID = -1;

        }
    }

    void UpdateWeaponSlotText()
    {
        var text = weaponSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = weaponSlot.id == 0 ? "" : data.items[weaponSlot.id].name;
    }

    void Update()
    {
        // Открытие/закрытие инвентаря по клавише I
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !backGround.activeSelf;
            backGround.SetActive(isActive);

            if (isActive)
            {
                UpdateInventory();
            }
        }

        // Перемещение предмета за курсором
        if (currentID != -1)
        {
            MoveObject();
        }
    }

    public int SearchForSameItem(Item item, int count)
    {
        for(int i = 0; i < maxCount; i++)
        {
            if (items[i].id == item.id)
            {
                if (items[i].count < item.maxCountInStack)
                {
                    items[i].count += count;
                    if (items[i].count > item.maxCountInStack)
                    {
                        count = items[i].count - item.maxCountInStack;
                        items[i].count = item.maxCountInStack;
                    }
                    else
                    {
                        count = 0;
                    }
                }
            }
        }
        if(count > 0)
        {
            for(int i = 0; i < maxCount; i++)
            {
                if (count == 0) break;
                if (items[i].id == 0)
                {
                    AddItem(i, item, count);
                    count = 0;
                }
            }
        }
        return count;
    }

    public void AddItem(int id, Item item, int count)
    {
        if (id < 0 || id >= items.Count)
        {
            Debug.LogError("Индекс вне диапазона списка items");
            return;
        }
        var invItem = items[id];
        if (invItem.itemGameObj == null)
        {
            Debug.LogError("itemGameObj не назначен");
            return;
        }

        invItem.id = item.id;
        invItem.count = count;

        var img = invItem.itemGameObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = item.img;
        }

        //Debug.Log("HERE1");
        if (invItem.itemGameObj.GetComponentInChildren<TMP_Text>() != null)
        {
            //Debug.Log("HERE2");
            if (count > 0 && item.id != 0)
            {
                invItem.itemGameObj.GetComponentInChildren<TMP_Text>().text = count.ToString();
            }
            else
            {
                invItem.itemGameObj.GetComponentInChildren<TMP_Text>().text = "";
            }
        }
    }

    public void AddInventoryItem(int id, ItemInventory invItem)
    {
        items[id].id = invItem.id;
        items[id].count = invItem.count;
        items[id].itemGameObj.GetComponent<Image>().sprite = data.items[invItem.id].img;
        if (invItem.count > 0 && invItem.id != 0)
        {
            items[id].itemGameObj.GetComponentInChildren<TMP_Text>().text = invItem.count.ToString();
        }
        else
        {
            items[id].itemGameObj.GetComponentInChildren<TMP_Text>().text = "";
        }
    }

    public void AddGraphics()
    {
        for(int i = 0; i < maxCount; i++)
        {
            GameObject newItem = Instantiate(gameObjShow, InventoryMainObject.transform) as GameObject;
            
            newItem.name = i.ToString();
            ItemInventory ii = new ItemInventory();
            ii.itemGameObj = newItem;
            RectTransform rt = newItem.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(0, 0, 0);
            rt.localScale = new Vector3(1, 1, 1);
            newItem.GetComponentInChildren<RectTransform>().localScale = new Vector3(1, 1, 1);

            Button tempButton = newItem.GetComponent<Button>();

            tempButton.onClick.AddListener(delegate { SelectObject(); });

            items.Add(ii);

        }
    }

    public void UpdateInventory()
    {
        for (int i = 0; i < maxCount; i++)
        {
            if (items[i].id != 0 && items[i].count > 0)
            {
                items[i].itemGameObj.GetComponentInChildren<TMP_Text>().text = items[i].count.ToString();
            }
            else
            {
                items[i].itemGameObj.GetComponentInChildren<TMP_Text>().text = "";
            }
            items[i].itemGameObj.GetComponent<Image>().sprite = data.items[items[i].id].img;
        }
    }

    public void UpdateMovingObjectUI()
    {
        TMP_Text text = movingObject.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = currentItem.count > 1 ? currentItem.count.ToString() : "";
        }
    }

    public void SelectObject()
    {
        int selectedID = int.Parse(es.currentSelectedGameObject.name);
        ItemInventory slotItem = items[selectedID];  // предмет в выбранной ячейке
        ItemInventory cursorItem = currentItem;      // предмет на курсоре

        // Если курсор пустой — просто берем предмет из слота
        if (currentID == -1)
        {
            Debug.Log($"currentItem.id = {currentItem.id}   selectedID = {selectedID} item = {items[selectedID].id}");
            if (items[selectedID].id == 0) return;
            currentID = selectedID;
            currentItem = CopyInventoryItem(slotItem);
            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateMovingObjectUI();
            // Очищаем слот
            AddItem(currentID, data.items[0], 0);
        }
        else
        {
            // Если предметы одинаковые и не пустые — пытаемся сложить
            if (cursorItem.id == slotItem.id && cursorItem.id != 0)
            {
                int maxStack = data.items[cursorItem.id].maxCountInStack;
                int totalCount = cursorItem.count + slotItem.count;

                if (totalCount <= maxStack)
                {
                    // Помещаем все в слот
                    slotItem.count = totalCount;
                    AddInventoryItem(selectedID, slotItem);

                    // Очистить курсор
                    currentID = -1;
                    currentItem = new ItemInventory();
                    movingObject.gameObject.SetActive(false);
                    UpdateMovingObjectUI();
                }
                else
                {
                    // Помещаем максимум в слот, остальное оставляем в курсе
                    slotItem.count = maxStack;
                    AddInventoryItem(selectedID, slotItem);

                    cursorItem.count = totalCount - maxStack;
                    currentItem = cursorItem;

                    // Курсор остается активным с остатком
                    currentID = selectedID;
                    movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
                    UpdateMovingObjectUI();
                }
            }
            else
            {
                // Предметы разные — меняем местами
                ItemInventory temp = CopyInventoryItem(slotItem);

                AddInventoryItem(selectedID, currentItem);
                currentItem = temp;

                if (currentItem.id == 0 || currentItem.count == 0)
                {
                    currentID = -1;
                    movingObject.gameObject.SetActive(false);
                }
                else
                {
                    currentID = selectedID;
                    movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
                }
                UpdateMovingObjectUI();
            }
        }
    }


    public void  MoveObject()
    {
        Vector3 pos = Input.mousePosition + offset;
        pos.z = InventoryMainObject.GetComponent<RectTransform>().position.z;
        movingObject.position = cam.ScreenToWorldPoint(pos);
    }

    public ItemInventory CopyInventoryItem(ItemInventory old)
    {
        ItemInventory New = new ItemInventory();
        
        New.id = old.id;
        New.itemGameObj = old.itemGameObj;
        New.count = old.count;

        return New;
    }
}

[System.Serializable]
public class ItemInventory
{
    public int id;
    public int count;
    public GameObject itemGameObj;
    public TMP_Text countText;  // заменил Text на TMP_Text
}