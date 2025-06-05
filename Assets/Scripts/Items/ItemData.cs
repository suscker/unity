using UnityEngine;

// Абстрактный базовый класс для любого предмета
public abstract class ItemData : ScriptableObject
{
    [Header("Общие параметры")]
    public int id;
    public int cost;
    public string itemName;
    public Sprite img;
    public int maxCountInStack = 1;
    public int weight;
}
