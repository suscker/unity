using UnityEngine;

// ������� CreateAssetMenu ��������� ��������� ������ ���� ScriptableObject �� ����
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItemData : ItemData
{
    [Header("��������� ������")]
    public int damage;
    [HideInInspector] public int currentAmmo; // ������� ���������� �������� (����� �������, ��� ���������� ��� ������)
    
    [Header("Fire Settings")]
    public bool isAutomatic = false;         // �������������� ����� ��������
    public float fireRate = 0.1f;            // ���������������� (��������� � �������)
    public int magazineSize = 30;            // ������ ��������
    public string ammoTipe = "9mm";          // ��� ��������

    [Header("Spread Settings")]
    public float baseSpread = 1f;            // ������� ������� (� ��������)
    public float maxSpread = 15f;             // ������������ ������� ��� ����������� ��������
    public float spreadIncreasePerShot = 2f;  // ���������� �������� �� �������
    public float spreadRecoveryRate = 5f;     // �������� �������������� �������� (��������/���)

    [Header("Movement Spread Settings")]
    public float maxMovementSpread = 7f;      // ����. ���. ������� ��� ��������
    public float movementSpreadMultiplier = 1f; // ��������� ������� ��������
}
