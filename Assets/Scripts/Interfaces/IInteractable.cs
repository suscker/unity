using UnityEngine;

/// <summary>
/// Интерфейс для объектов, с которыми может взаимодействовать игрок
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Метод, вызываемый при взаимодействии с объектом
    /// </summary>
    /// <param name="playerInventory">Инвентарь игрока</param>
    void Interact(Inventory playerInventory);
} 