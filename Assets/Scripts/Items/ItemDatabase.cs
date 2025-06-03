using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Список всех предметов в проекте")]
    public List<ItemData> items = new List<ItemData>();
}
