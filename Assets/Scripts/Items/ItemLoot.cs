using UnityEngine;

[CreateAssetMenu(fileName = "NewLoot", menuName = "Items/Loot")]
public class ItemLoot : ItemData
{
    public override void Use()
    {
        Debug.Log($"Nothing happened with Loot");
    }
}
