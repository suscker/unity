using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("������ ���� ��������� � �������")]
    public List<ItemData> items = new List<ItemData>();
}
