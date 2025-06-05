using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerHealth playerHealth;

    [Tooltip("ItemDatabase")]
    public ItemDatabase data;
    public List<ItemInventory> items = new List<ItemInventory>();

    [Header("UI")]
    [Tooltip("������ �� Text, ��� ���������� ��� (� ��)")]
    public TMP_Text weightText;

    public GameObject gameObjShow;

    public GameObject InventoryMainObject;

    public int maxCount;

    public Camera cam;
    public EventSystem es;

    public int currentID;
    public ItemInventory currentItem;

    public RectTransform movingObject;
    public Vector2 offset;

    public GameObject backGround;

    //public ItemInventory weaponSlot = new ItemInventory();
    public WeaponInventory weaponSlot = null; 
    public GameObject weaponSlotObject; 
    public Sprite weaponSlotDefaultSprite;

    //public ItemInventory armorSlot = new ItemInventory();
    public ArmorInventory armorSlot;
    public GameObject armorSlotObject;
    public Sprite armorSlotDefaultSprite;

    public HealInventory healSlot = null;
    public GameObject healSlotObject;
    public Sprite healSlotDefaultSprite;

    [Header("External Inventories")]
    public GameObject crateInventoryUI; // ������ �� UI ��������� �����


    public void Start()
    {
        Debug.Log($"ItemDatabase size = {data.items.Count}");
        if (items.Count == 0)
        {
            AddGraphics();
        }
        weaponSlot.itemGameObj = weaponSlotObject;
        weaponSlot.id = 0;

        armorSlot = new ArmorInventory(
        new ItemInventory { id = 0, itemGameObj = armorSlotObject },
        new ItemArmor()
        );
        healSlot = new HealInventory(
        new ItemInventory { id = 0, itemGameObj = armorSlotObject },
        new ItemHeal()
        );
        weaponSlot = new WeaponInventory(
        new ItemInventory { id = 0, itemGameObj = weaponSlotObject },
        new WeaponItemData()
        );
        for (int i = 0; i < maxCount; i++)
        {
            AddItem(i, data.items[0], 0);
        }


        /*
         * ��� 
         * ���
         * �����
        */
        ///Debug.Log("Ya tyt");
        for (int i = 0; i < maxCount; i++)
        {
            int number = Random.Range(0, data.items.Count);
            ItemData rndItem = data.items[number];

            // ���� ������ ����� � ������ � ������ ArmorInventory � ������� �� random durability
            if (rndItem is ItemArmor armorData)
            {
                AddItem(i, rndItem, 1);

                // � ���� ����� ����� ������������� ��������� ��������� (���� �����)
                if (items[i] is ArmorInventory aSlot)
                {
                    aSlot.currentDurability = Random.Range(1, aSlot.maxDurability + 1);

                    // � ����� ��������� ����� (�� �� ����� ���� ��� �������������
                    // AddItem ��� ��������� ��������, � ����� �� ������ ������������):
                    var txt = aSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
                    if (txt != null)
                        txt.text = $"{aSlot.currentDurability}/{aSlot.maxDurability}";
                }
            }
            else
            {
                // ������� �������
                int rndCount = rndItem.maxCountInStack > 1
                               ? Random.Range(1, rndItem.maxCountInStack)
                               : 1;
                AddItem(i, rndItem, rndCount);
            }
        }

        UpdateInventory();
    }

    public void OpenInventoryWithCrate(GameObject crateUI)
    {
        crateInventoryUI = crateUI;
        backGround.SetActive(true);
        if (crateInventoryUI != null)
        {
            crateInventoryUI.SetActive(true);
        }
        UpdateInventory();
        UpdateWeaponSlotText();
    }

    public void CloseInventory()
    {
        backGround.SetActive(false);
        if (crateInventoryUI != null)
        {
            crateInventoryUI.SetActive(false);
            crateInventoryUI = null;
        }
    }

    ///

    bool IsHeal(ItemData item)
    {
        if (item is ItemHeal) return true;
        return false;
    }

    public void OnHealDestroyed()
    {
        // ���������� ���� �������
        healSlot = new HealInventory(
            new ItemInventory { id = 0, itemGameObj = healSlotObject },
            new ItemHeal()
        );

        healSlot.itemGameObj.GetComponent<Image>().sprite = healSlotDefaultSprite;
        UpdateHealSlotText();

        Debug.Log("������� ���������!");
    }

    public void EquipHeal(ItemInventory baseItem)
    {
        if (baseItem == null || baseItem.id == 0)
            return;

        var healData = data.items[baseItem.id] as ItemHeal;
        if (healData == null)
            return;

        healSlot = new HealInventory(baseItem, healData);

        healSlot.itemGameObj.GetComponent<Image>().sprite = healData.img;
        UpdateHealSlotText();
    }

    public void OnHealSlotClicked()
    {
        if (currentID == -1)
        {
            // ����� �� �����
            if (healSlot == null || healSlot.id == 0) return;

            currentItem = new HealInventory(
                new ItemInventory { id = healSlot.id, count = healSlot.count },
                data.items[healSlot.id] as ItemHeal
            )
            {
                currentHeal = healSlot.currentHeal,
                maxHeal = healSlot.maxHeal
            };

            // ������� ����
            healSlot.id = 0;
            healSlot.currentHeal = 0;
            healSlot.maxHeal = 0;
            UpdateHealSlotText();

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateMovingObjectUI();

            currentID = -2;
        }
        else
        {
            // ������ � ����
            if (!(currentItem is HealInventory healItem)) return;

            // ���� � ����� ��� ���� ������� - ������� ��� � ���������
            if (healSlot.id != 0)
            {
                int remaining = SearchForSameItem(data.items[healSlot.id], 1);
                if (remaining > 0) Debug.Log("�� ������� ������� ������� �� �����");
            }

            // ��������� ����
            healSlot = new HealInventory(
                new ItemInventory { id = healItem.id, count = 1 },
                data.items[healItem.id] as ItemHeal
            ) 
            {
                currentHeal = healItem.currentHeal,
                maxHeal = healItem.maxHeal
            };

            UpdateHealSlotText();

            // ������� ������
            currentItem = null;
            movingObject.gameObject.SetActive(false);
            currentID = -1;
        }
    }

    public void UpdateHealSlotText()
    {
        if (healSlot == null || healSlot.id == 0)
        {
            // �������� ����
            healSlotObject.GetComponent<Image>().sprite = healSlotDefaultSprite;
            var text = healSlotObject.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = "";
            return;
        }

        var textUI = healSlotObject.GetComponentInChildren<TMP_Text>();
        if (textUI != null)
        {
            textUI.text = $"{data.items[healSlot.id].name} ({healSlot.currentHeal}/{healSlot.maxHeal})";
        }

        healSlotObject.GetComponent<Image>().sprite = data.items[healSlot.id].img;
    }



    ///
    bool IsArmor(ItemData item)
    {
        if (item is ItemArmor) return true;
        return false;
    }

    public void OnArmorDestroyed()
    {
        // ���������� ���� �����
        armorSlot = new ArmorInventory(
            new ItemInventory { id = 0, itemGameObj = armorSlotObject },
            new ItemArmor()
        );

        armorSlot.itemGameObj.GetComponent<Image>().sprite = armorSlotDefaultSprite;
        UpdateArmorSlotText();

        Debug.Log("����� ���������!");
    }

    public void EquipArmor(ItemData item)
    {
        ItemArmor armorItem = item as ItemArmor;
        if (armorItem != null)
        {
            // ������� ArmorInventory �� �������� ItemInventory
            armorSlot = new ArmorInventory(
                new ItemInventory
                {
                    id = item.id,
                    count = 1,
                    itemGameObj = armorSlotObject
                },
                armorItem
            );

            armorSlot.itemGameObj.GetComponent<Image>().sprite = item.img;
            UpdateArmorSlotText();

            if (playerHealth != null)
            {
                playerHealth.EquipArmor(armorItem, armorSlot);
            }
        }
    }

    public void OnArmorSlotClicked()
    {
        // ���� �� ������ ������, � � ����� ����� ���-�� �����:
        if (currentID == -1)
        {
            if (armorSlot.id == 0) return;

            // �������� ������ � currentItem (����� ������� �������, ���� �������� ����):
            currentItem = CopyInventoryItem(armorSlot);

            // ������� ����� � ���������: 
            if (playerHealth != null)
                playerHealth.RemoveArmor();

            // ������� UI-���� (������ �������������):
            armorSlot.id = 0;
            armorSlot.count = 0;
            armorSlot.currentDurability = 0;
            armorSlot.maxDurability = 0;
            armorSlot.itemGameObj.GetComponent<Image>().sprite = armorSlotDefaultSprite;
            UpdateArmorSlotText();

            // �������� ������ ����� �� ��������:
            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            var txt = movingObject.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = $"{(currentItem as ArmorInventory).currentDurability}/{(currentItem as ArmorInventory).maxDurability}";

            currentID = -2;
        }
        else
        {
            // ���� � ������� ����� ����� (� �� ���-�� ������):
            if (!IsArmor(data.items[currentItem.id])) return;

            // ���� � ����� ���� ������ �����, ������ � � ���������:
            if (armorSlot.id != 0)
            {
                int rem = SearchForSameItem(data.items[armorSlot.id], armorSlot.count);
                if (rem > 0)
                    Debug.LogWarning("�� ������� ������� ����� ���������. ��������: " + rem);

                if (playerHealth != null)
                    playerHealth.RemoveArmor();
            }

            // ��������� ����� �� ������� � ���� � �� �� ������ ����� ArmorInventory:
            // ������ ����� �������� ���� ������ ����������� � currentItem ���������:
            ArmorInventory armorFromCursor = currentItem as ArmorInventory;
            armorSlot.id = armorFromCursor.id;
            armorSlot.count = armorFromCursor.count;
            armorSlot.currentDurability = armorFromCursor.currentDurability;
            armorSlot.maxDurability = armorFromCursor.maxDurability;
            armorSlot.itemGameObj.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateArmorSlotText();

            // ������� � PlayerHealth �� �� ����� ������� � ��� ����������������� ����������:
            if (playerHealth != null)
                playerHealth.EquipArmor(data.items[armorSlot.id] as ItemArmor, armorSlot);

            // ������� �������
            currentItem = new ItemInventory();
            movingObject.gameObject.SetActive(false);
            currentID = -1;
        }
    }

    public void UpdateArmorSlotText()
    {
        var text = armorSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            if (armorSlot.id != 0)
            {
                // ���������� ���������: �������/������������
                text.text = $"{armorSlot.currentDurability}/{armorSlot.maxDurability}";
            }
            else
            {
                text.text = "";
            }
        }
    }

    ///

    bool IsWeapon(ItemData item)
    {
        if (item is WeaponItemData) return true;
        return false;
    }

    public void EquipWeapon(ItemData item)
    {
        if (item == null || item.id == 0)
            return;

        weaponSlot.id = item.id;
        weaponSlot.count = 1;
        weaponSlot.itemGameObj.GetComponent<Image>().sprite = item.img;

        var text = weaponSlot.itemGameObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = item.name; // ��� ������ ""
    }

    public void OnWeaponSlotClicked()
    {
        Debug.Log("Weapon slot clicked!");
        if (currentID == -1)
        {
            // � ���� ������ - ����� �� weaponSlot
            if (weaponSlot.id == 0) return;

            currentItem = CopyInventoryItem(weaponSlot);

            // ������� ����
            weaponSlot = new WeaponInventory(
                new ItemInventory { id = 0, itemGameObj = weaponSlotObject },
                null
            );

            UpdateWeaponSlotUI(); // ���������� ����� ���������� UI

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateMovingObjectUI();

            currentID = -2;
        }
        else
        {
            // ���� ������� � ����
            if (!(data.items[currentItem.id] is WeaponItemData)) return;

            // ���� � ����� ��� ���� ������� - ������� ��� � ���������
            if (weaponSlot.id != 0)
            {
                int freeIndex = FindFreeSlot();
                if (freeIndex != -1)
                {
                    AddInventoryItem(freeIndex, weaponSlot);
                }
                else
                {
                    Debug.LogWarning("��������� �����! �� ���� ������� ������.");
                    return;
                }
            }

            // ����������� ������ (FIXED: ��������� ������� �������)
            WeaponItemData weaponData = data.items[currentItem.id] as WeaponItemData;
            weaponSlot = new WeaponInventory(
                new ItemInventory
                {
                    id = currentItem.id,
                    count = 1,
                    itemGameObj = weaponSlotObject // ���������� �����!
                },
                weaponData
            );

            // ��������� ������� �������
            if (currentItem is WeaponInventory weaponItem)
            {
                weaponSlot.currentAmmo = weaponItem.currentAmmo;
            }
            else
            {
                weaponSlot.currentAmmo = weaponData.magazineSize;
            }

            UpdateWeaponSlotUI(); // ���������� ����� ���������� UI

            // �������� ������
            currentItem = new ItemInventory();
            movingObject.gameObject.SetActive(false);
            currentID = -1;
        }
    }

    public void UpdateWeaponSlotText()
    {
        var text = weaponSlotObject.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            if (weaponSlot.id != 0)
            {
                text.text = $"{weaponSlot.currentAmmo}/{weaponSlot.magazineSize}";
            }
            else
            {
                text.text = "";
            }
        }
    }

    ///

    public int GetTotalWeightInGrams()
    {
        int totalWeight = 0;
        for(int i=0;i<maxCount;i++)
        {
            if (items[i].id != 0)
            {

                totalWeight += data.items[items[i].id].weight * items[i].count;
            }
        }
        if (weaponSlot != null && weaponSlot.id != 0)
            totalWeight += data.items[weaponSlot.id].weight * weaponSlot.count;

        if (armorSlot.id != 0 && armorSlot.count > 0)
            totalWeight += data.items[armorSlot.id].weight * armorSlot.count;

        if (healSlot.id != 0 && healSlot.count > 0)
            totalWeight += data.items[healSlot.id].weight * healSlot.count;
        ///Debug.Log($"currentID = {currentID}   currentItem.id= {currentItem.id}");
        if (currentID != -1)
            totalWeight +=  data.items[currentItem.id].weight * currentItem.count;
        return totalWeight;
    }

    public float GetTotalWeightInKg()
    {
        float totalWeight = GetTotalWeightInGrams() / 1000f;
        weightText.text = $"{totalWeight:F1} kg";
        return totalWeight;
    }

    void Update()
    {
        // ��������/�������� ��������� �� ������� I
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = !backGround.activeSelf;
            backGround.SetActive(isActive);

            if (isActive)
            {
                UpdateInventory();
            }
        }

        // ����������� �������� �� ��������
        if (currentID != -1)
        {
            MoveObject();
        }
    }


    public int SearchForSameItem(ItemData item, int count)
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

    public void AddItem(int slotIndex, ItemData item, int count)
    {
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            Debug.LogError("������ ��� ��������� ������ items");
            return;
        }

        GameObject cellGo = items[slotIndex].itemGameObj;
        if (cellGo == null)
        {
            Debug.LogError("itemGameObj �� ��������");
            return;
        }

        if (item == null || item.id == 0 || count <= 0)
        {
            items[slotIndex] = new ItemInventory
            {
                id = 0,
                count = 0,
                itemGameObj = cellGo
            };

            var imgClear = cellGo.GetComponent<Image>();
            if (imgClear != null)
                imgClear.sprite = data.items[0].img;

            var txtClear = cellGo.GetComponentInChildren<TMP_Text>();
            if (txtClear != null)
                txtClear.text = "";

            return;
        }

        if (item is ItemArmor armorData)
        {
            var newArmorSlot = new ArmorInventory(
                new ItemInventory { id = item.id, count = 1, itemGameObj = cellGo },
                armorData
            );

            cellGo.GetComponent<Image>().sprite = item.img;
            var txt = cellGo.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = $"{newArmorSlot.currentDurability}/{newArmorSlot.maxDurability}";

            items[slotIndex] = newArmorSlot;
        }
        else if (item is WeaponItemData weaponData)
        {
            var newWeaponSlot = new WeaponInventory(
                new ItemInventory { id = item.id, count = count, itemGameObj = cellGo },
                weaponData
            );

            cellGo.GetComponent<Image>().sprite = item.img;
            var txt = cellGo.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = $"{newWeaponSlot.currentAmmo}/{newWeaponSlot.magazineSize}";

            items[slotIndex] = newWeaponSlot;
        }
        else if (item is ItemHeal healData) // ��������� �������� ��� �������
        {
            var newHealSlot = new HealInventory(
                new ItemInventory { id = item.id, count = 1, itemGameObj = cellGo },
                healData
            );

            cellGo.GetComponent<Image>().sprite = item.img;
            var txt = cellGo.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = $"{newHealSlot.currentHeal}/{newHealSlot.maxHeal}";

            items[slotIndex] = newHealSlot;
        }
        else
        {
            var newItem = new ItemInventory
            {
                id = item.id,
                count = count,
                itemGameObj = cellGo
            };

            cellGo.GetComponent<Image>().sprite = item.img;
            var txt = cellGo.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = (count > 1 ? count.ToString() : "");

            items[slotIndex] = newItem;
        }
    }


    public void AddInventoryItem(int slotIndex, ItemInventory invItem)
    {
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            Debug.LogError("������ ��� ��������� ������ items");
            return;
        }

        GameObject cellGo = items[slotIndex].itemGameObj;
        if (cellGo == null)
        {
            Debug.LogError("itemGameObj �� ��������");
            return;
        }

        if (invItem is ArmorInventory armorOld)
        {
            ItemData template = data.items[armorOld.id];
            if (template is ItemArmor armorData)
            {
                ArmorInventory newArmorSlot = new ArmorInventory(
                    new ItemInventory
                    {
                        id = armorOld.id,
                        count = armorOld.count,
                        itemGameObj = cellGo
                    },
                    armorData
                );
                newArmorSlot.currentDurability = armorOld.currentDurability;
                newArmorSlot.maxDurability = armorOld.maxDurability;

                cellGo.GetComponent<Image>().sprite = armorData.img;
                var txt = cellGo.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                    txt.text = $"{newArmorSlot.currentDurability}/{newArmorSlot.maxDurability}";

                items[slotIndex] = newArmorSlot;
                return;
            }
            else
            {
                Debug.LogError($"Item � id={armorOld.id} �� �������� ItemArmor");
            }
        }
        else if (invItem is WeaponInventory weaponOld)
        {
            ItemData template = data.items[weaponOld.id];
            if (template is WeaponItemData weaponData)
            {
                WeaponInventory newWeaponSlot = new WeaponInventory(
                    new ItemInventory
                    {
                        id = weaponOld.id,
                        count = weaponOld.count,
                        itemGameObj = cellGo
                    },
                    weaponData
                )
                {
                    currentAmmo = weaponOld.currentAmmo,
                    magazineSize = weaponOld.magazineSize
                };

                cellGo.GetComponent<Image>().sprite = weaponData.img;
                var txt = cellGo.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                    txt.text = $"{newWeaponSlot.currentAmmo}/{newWeaponSlot.magazineSize}";

                items[slotIndex] = newWeaponSlot;
                return;
            }
        }
        else if (invItem is HealInventory healOld)
        {
            ItemData template = data.items[healOld.id];
            if (template is ItemHeal healData)
            {
                HealInventory newHealSlot = new HealInventory(
                    new ItemInventory
                    {
                        id = healOld.id,
                        count = healOld.count,
                        itemGameObj = cellGo // ��������� ������ �� ������!
                    },
                    healData
                )
                {
                    currentHeal = healOld.currentHeal,
                    maxHeal = healOld.maxHeal
                };

                cellGo.GetComponent<Image>().sprite = healData.img;
                var txt = cellGo.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = $"{newHealSlot.currentHeal}/{newHealSlot.maxHeal}";

                items[slotIndex] = newHealSlot;
                return;
            }
        }

        // ������� ItemInventory
        ItemInventory newItem = new ItemInventory
        {
            id = invItem.id,
            count = invItem.count,
            itemGameObj = cellGo
        };

        cellGo.GetComponent<Image>().sprite = data.items[invItem.id].img;
        var txt2 = cellGo.GetComponentInChildren<TMP_Text>();
        if (txt2 != null)
            txt2.text = (invItem.count > 1 ? invItem.count.ToString() : "");

        items[slotIndex] = newItem;
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
            var go = items[i].itemGameObj;
            var imgComp = go.GetComponent<Image>();
            var txtComp = go.GetComponentInChildren<TMP_Text>();

            // ��������� ������ (���� ���� id == 0, data.items[0].img � �������� ��������)
            if (imgComp != null)
                imgComp.sprite = data.items[items[i].id].img;

            // ���� ���� �� ������ (id != 0 � count > 0)
            if (items[i].id != 0 && items[i].count > 0)
            {
                // ��������� ��� ��������
                if (items[i] is WeaponInventory weaponSlot)
                {
                    if (txtComp != null)
                        txtComp.text = $"{weaponSlot.currentAmmo}/{weaponSlot.magazineSize}";
                }
                else if (items[i] is ArmorInventory armorSlot)
                {
                    if (txtComp != null)
                        txtComp.text = $"{armorSlot.currentDurability}/{armorSlot.maxDurability}";
                }
                else if (items[i] is HealInventory healSlot)
                {
                    if (txtComp != null)
                        txtComp.text = $"{healSlot.currentHeal}/{healSlot.maxHeal}";
                }
                else
                {
                    if (txtComp != null)
                        txtComp.text = items[i].count > 1 ? items[i].count.ToString() : "";
                }
            }
            else
            {
                // ���� ������ � ������� �����
                if (txtComp != null)
                    txtComp.text = "";
            }
        }
    }

    /// <summary>
    /// ��������� ������ � ����� � ������ ����� ������.
    /// </summary>
    public void UpdateWeaponSlotUI()
    {
        Image img = weaponSlotObject.GetComponent<Image>();
        TMP_Text txt = weaponSlotObject.GetComponentInChildren<TMP_Text>();

        if (weaponSlot.id != 0)
        {
            img.sprite = data.items[weaponSlot.id].img;
            txt.text = $"{weaponSlot.currentAmmo}/{weaponSlot.magazineSize}";
        }
        else
        {
            img.sprite = weaponSlotDefaultSprite;
            txt.text = "";
        }
    }
    public void UpdateMovingObjectUI()
    {
        TMP_Text text = movingObject.GetComponentInChildren<TMP_Text>();
        if (text == null) return;

        if (currentItem is WeaponInventory weaponCursor)
        {
            text.text = $"{weaponCursor.currentAmmo}/{weaponCursor.magazineSize}";
        }
        else if (currentItem is ArmorInventory armorCursor)
        {
            // ��� ����� ���������� �������/������������ ���������
            text.text = $"{armorCursor.currentDurability}/{armorCursor.maxDurability}";
        }
        else if (currentItem is HealInventory healCursor)
        {
            // ��� ������� ���������� ������� � ������������ �������
            text.text = $"{healCursor.currentHeal}/{healCursor.maxHeal}";
        }
        else
        {
            // ��� ������� ��������� ���������� ����������
            text.text = currentItem.count > 1 ? currentItem.count.ToString() : "";
        }
    }

    private int FindFreeSlot()
    {
        for (int i = 0; i < maxCount; i++)
        {
            if (items[i].id == 0)
                return i;
        }
        return -1;
    }

    public void SelectObject()
    {
        if (currentItem is HealInventory healCursor)
        {
            Debug.Log($"Moving heal item: {healCursor.currentHeal}/{healCursor.maxHeal}");
        }
        if (!int.TryParse(es.currentSelectedGameObject.name, out int index))
        {
            Debug.Log($"cant pars = {es.currentSelectedGameObject.name}");
            return;
        }
        int selectedID = int.Parse(es.currentSelectedGameObject.name);
        ItemInventory slotItem = items[selectedID];  // ������� � ��������� ������
        ItemInventory cursorItem = currentItem;      // ������� �� �������

        // ���� ������ ������ � ������ ����� ������� �� �����

        if (currentID == -1)
        {
            if (items[selectedID].id == 0) return;

            currentID = selectedID;
            currentItem = CopyInventoryItem(items[selectedID]); // �������� � ����������� ������

            // ������� ����, �� ��������� ��� UI-������
            AddItem(currentID, data.items[0], 0);

            movingObject.gameObject.SetActive(true);
            movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
            UpdateMovingObjectUI();
        }
        else
        {
            // ���� �������� ���������� � �� ������ � �������� �������
            if (cursorItem.id == slotItem.id && cursorItem.id != 0)
            {
                int maxStack = data.items[cursorItem.id].maxCountInStack;
                int totalCount = cursorItem.count + slotItem.count;

                if (totalCount <= maxStack)
                {
                    // �������� ��� � ����
                    slotItem.count = totalCount;
                    AddInventoryItem(selectedID, slotItem);

                    // �������� ������
                    currentID = -1;
                    currentItem = new ItemInventory();
                    movingObject.gameObject.SetActive(false);
                    UpdateMovingObjectUI();
                }
                else
                {
                    // �������� �������� � ����, ��������� ��������� � �����
                    slotItem.count = maxStack;
                    AddInventoryItem(selectedID, slotItem);

                    cursorItem.count = totalCount - maxStack;
                    currentItem = cursorItem;

                    // ������ �������� �������� � ��������
                    currentID = selectedID;
                    movingObject.GetComponent<Image>().sprite = data.items[currentItem.id].img;
                    UpdateMovingObjectUI();
                }
            }
            else
            {
                // �������� ������ � ������ �������
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
        Vector2 mousePos = Input.mousePosition;
        movingObject.position = mousePos + offset;
    }

    public ItemInventory CopyInventoryItem(ItemInventory old)
    {

        // ��� ������
        if (old is WeaponInventory weaponOld)
        {
            WeaponInventory newWeapon = new WeaponInventory(
                new ItemInventory
                {
                    id = weaponOld.id,
                    count = weaponOld.count,
                    itemGameObj = weaponOld.itemGameObj
                },
                data.items[weaponOld.id] as WeaponItemData
            )
            {
                currentAmmo = weaponOld.currentAmmo,
                magazineSize = weaponOld.magazineSize
            };
            return newWeapon;
        }
        // ��� �����
        if (old is ArmorInventory armorOld)
        {
            ArmorInventory newArmor = new ArmorInventory(
                new ItemInventory
                {
                    id = armorOld.id,
                    count = armorOld.count,
                    itemGameObj = armorOld.itemGameObj // ��������� ������!
                },
                data.items[armorOld.id] as ItemArmor
            )
            {
                currentDurability = armorOld.currentDurability,
                maxDurability = armorOld.maxDurability
            };
            return newArmor;
        }

        // ��� �������
        if (old is HealInventory healOld)
        {
            HealInventory newHeal = new HealInventory(
                new ItemInventory
                {
                    id = healOld.id,
                    count = healOld.count,
                    itemGameObj = healOld.itemGameObj // ��������� ������!
                },
                data.items[healOld.id] as ItemHeal
            )
            {
                currentHeal = healOld.currentHeal,
                maxHeal = healOld.maxHeal
            };
            return newHeal;
        }

        // ��� ������� ���������
        ItemInventory New = new ItemInventory
        {
            id = old.id,
            count = old.count,
            itemGameObj = old.itemGameObj // ��������� ������!
        };
        return New;
    }


}

[System.Serializable]
public class ItemInventory
{
    public int id;
    public int count;
    public GameObject itemGameObj;
    public TMP_Text countText;  // ������� Text �� TMP_Text
}

[System.Serializable]
public class ArmorInventory : ItemInventory
{
    public int currentDurability; // ������� ��������� �����
    public int maxDurability;     // ������������ ���������

    // ����������� ��� ����������� �������� ��������
    public ArmorInventory(ItemInventory baseItem, ItemArmor armorData)
    {
        id = baseItem.id;
        count = baseItem.count;
        itemGameObj = baseItem.itemGameObj;
        currentDurability = armorData.currentHP;
        maxDurability = armorData.maxHP;
    }
}

[System.Serializable]
public class HealInventory : ItemInventory
{
    public int currentHeal; // ������� ��������� �������
    public int maxHeal;     // ������������ ���������

    // ����������� ��� ����������� �������� �������� �������
    public HealInventory(ItemInventory baseItem, ItemHeal healData)
    {
        id = baseItem.id;
        count = baseItem.count;
        itemGameObj = baseItem.itemGameObj;
        currentHeal = healData.currentHeal;
        maxHeal = healData.maxHeal;
    }
}

[System.Serializable]
public class WeaponInventory : ItemInventory
{
    public int currentAmmo;     // ������� ���������� ��������
    public int magazineSize;     // ������ ��������

    public WeaponInventory(ItemInventory baseItem, WeaponItemData weaponData)
    {
        id = baseItem.id;
        count = baseItem.count;
        itemGameObj = baseItem.itemGameObj;

        if (weaponData != null)
        {
            magazineSize = weaponData.magazineSize;
            currentAmmo = weaponData.magazineSize; // �� ��������� ������ �������
        }
        else
        {
            magazineSize = 0;
            currentAmmo = 0;
        }
    }
}