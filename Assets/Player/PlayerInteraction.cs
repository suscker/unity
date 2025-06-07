using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 2f;    // Дистанция взаимодействия
    public LayerMask interactableLayers;      // Слои, с которыми можно взаимодействовать
    private List<GameObject> slotInstances = new List<GameObject>();

    private Camera mainCamera;
    private Inventory playerInventory;

    private void Start()
    {
        mainCamera = Camera.main;
        // Ищем инвентарь на том же объекте (Player)
        playerInventory = GetComponent<Inventory>();
        
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInteraction: Inventory component not found on player!");
        }
    }


    public Crate TryInteract()
    {
        Vector2 origin = transform.position;
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - origin).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactionDistance, interactableLayers);

        if (hit.collider != null)
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
        else
            Debug.Log("Raycast missed!");

        if (hit.collider != null)
        {
            if(hit.collider.GetComponent<Crate>() != null)
            return hit.collider.GetComponent<Crate>();
        }
        return null;
    }

    // Визуализация дистанции взаимодействия в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 