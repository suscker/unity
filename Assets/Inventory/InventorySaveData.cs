using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items = new List<ItemSaveData>();
    public WeaponSaveData weaponSlot;
    public ArmorSaveData armorSlot;
    public HealSaveData healSlot;
    public float totalWeight;

    [Serializable]
    public class ItemSaveData
    {
        public int id;
        public int count;
        public int currentDurability;
        public int maxDurability;
        public int currentAmmo;
        public int magazineSize;
        public int currentHeal;
        public int maxHeal;
        public string itemType; // "Weapon", "Armor", "Heal", "Item"
    }

    [Serializable]
    public class WeaponSaveData
    {
        public int id;
        public int currentAmmo;
        public int magazineSize;
    }

    [Serializable]
    public class ArmorSaveData
    {
        public int id;
        public int currentDurability;
        public int maxDurability;
    }

    [Serializable]
    public class HealSaveData
    {
        public int id;
        public int currentHeal;
        public int maxHeal;
    }
}

[Serializable]
public class CrateSaveData
{
    public string crateId;
    public List<InventorySaveData.ItemSaveData> items = new List<InventorySaveData.ItemSaveData>();
}

[System.Serializable]
public class EnemySaveData
{
    public string enemyId;
    public float[] position = new float[3];
    public int maxHealth;
    public int currentHealth;
    public bool isInvincible;
}

[Serializable]
public class GameSaveData
{
    public InventorySaveData playerInventory;
    public List<CrateSaveData> crates = new List<CrateSaveData>();
    public string saveDate;

    public float[] playerPosition = new float[3]; // x, y, z
    public float[] playerVelocity = new float[2]; // x, y
    public List<EnemySaveData> enemies = new List<EnemySaveData>();
} 