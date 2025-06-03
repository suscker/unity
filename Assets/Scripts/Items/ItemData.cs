using UnityEngine;

// ����������� ������� ����� ��� ������ ��������
public abstract class ItemData : ScriptableObject
{
    [Header("����� ���������")]
    public int id;
    public int cost;
    public string itemName;
    public Sprite img;
    public int maxCountInStack = 1;
    public int weight;

    // ������� ����������� ����� ������������� ��������
    // ��� ������ ����� ��������� ����� ��������������.
    public virtual void Use()
    {
        Debug.Log($"Using item: {itemName}");
    }
}
