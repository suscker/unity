using UnityEngine;

public enum AmmoType
{
    sniperRifle,
    automaticRifle,
    // ... ��� ������� �������� ������ ���� ��������
}

// ������� CreateAssetMenu ��������� ��������� ������ ���� ScriptableObject �� ����
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("��������� ������")]
    public int damage;
    public int magazineSize;
    [HideInInspector] public int currentAmmo; // ������� ���������� �������� (����� �������, ��� ���������� ��� ������)
    public float fireRate;
    public AmmoType ammoType;

    public override void Use()
    {
        Debug.Log($"Firing weapon: {itemName}");
        // ����� ����� �������� ������ ��������, �� ������ �� �� ��������� � ����� ���,
        // � ���������� � WeaponSystem (��. �����).
    }
}
