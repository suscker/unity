using UnityEngine;
using System.Collections.Generic;

public class DataBase : MonoBehaviour
{
    public List<Item> items = new List<Item>();
}
public enum ItemType { Weapon, Armor, Heal, Other }
[System.Serializable]
public class Item
{
    public int id;
    public int maxCountInStack;
    public string name;
    public Sprite img;
    public int weight;
    public ItemType type;
} 
