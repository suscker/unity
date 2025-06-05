using UnityEngine;

[CreateAssetMenu(fileName = "NewItemHeal", menuName = "Items/Heal")]
public class ItemHeal : ItemData
{
    public int currentHeal;
    public int maxHeal;
    public float healHoldTime;
}
