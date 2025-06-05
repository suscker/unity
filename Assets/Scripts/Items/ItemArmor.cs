using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Items/Armor")]
public class ItemArmor : ItemData
{
    [Header("��������� �����")]
    public int maxHP;
    public int currentHP;
    public int efficiency;
}
