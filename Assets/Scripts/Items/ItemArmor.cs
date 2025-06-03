using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Items/Armor")]
public class ItemArmor : ItemData
{
    [Header("Параметры брони")]
    public int maxHP;
    public int currentHP;
    public int efficiency;
    public override void Use()
    {
        Debug.Log($"Nothing happened with armor");
    }
}
