using UnityEngine;

[CreateAssetMenu(fileName = "NewNothing", menuName = "Items/Nothing")]
public class ItemNothing : ItemData
{
    public override void Use()
    {
        Debug.Log($"Nothing happened");
    }
}
