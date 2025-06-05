using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 2f;    // Дистанция взаимодействия
    public LayerMask interactableLayers;      // Слои, с которыми можно взаимодействовать
    public KeyCode interactKey = KeyCode.E;   // Клавиша взаимодействия
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

    private void Update()
    {
        // Проверяем нажатие клавиши взаимодействия
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
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
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(playerInventory);
            }
        }
    }

    // Визуализация дистанции взаимодействия в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
} 