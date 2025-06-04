using UnityEngine;
using UnityEngine.SceneManagement;  // требуется для перезагрузки сцены
using UnityEngine.UI;              // для работы с кнопками как UI-компонентами
using System.Collections.Generic;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();
    [Header("PlayerMovement2D Settings")]
    public PlayerMovement2D playerMovement;
    [Header("Health Settings")]
    public int maxHealth = 500;
    private float healHoldTimer = 0f;
    private bool isHealing = false;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    [Header("Links")]
    [Tooltip("Ссылка на Inventory (назначается в инспекторе)")]
    public Inventory inventory;

    [Header("Armor Reference")]
    public ItemArmor equippedArmor;
    public ArmorInventory armorSlot;

    [Header("Game Over UI")]
    [Tooltip("Ссылка на панель 'Game Over' (Canvas → GameOverPanel)")]
    public GameObject gameOverPanel;

    [Tooltip("Кнопка 'Начать заново'")]
    public Button restartButton;

    [Tooltip("Кнопка 'Выйти из игры'")]
    public Button quitButton;

    private bool isDead = false;

    private void Start()
    {
        keyStates.Add(KeyCode.LeftShift, false);
        keyStates.Add(KeyCode.Z, false);
        keyStates.Add(KeyCode.H, false);

        currentHealth = maxHealth;
        Debug.Log($"Health initialized: {currentHealth}/{maxHealth}");

        // Скрываем панель GameOver
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Подписываемся на нажатия кнопок, если они назначены
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void Update()
    {
        // Блокируем ввод, если персонаж уже мёртв
        if (isDead)
            return;

        if (!keyStates.ContainsKey(KeyCode.Z))
            keyStates.Add(KeyCode.Z, false);
        CheckKey(KeyCode.Z);
        if (!keyStates.ContainsKey(KeyCode.Z))
            keyStates.Add(KeyCode.LeftShift, false);
        CheckKey(KeyCode.LeftShift);
        HandleHealHold();
    }

    void CheckKey(KeyCode key)
    {
        if (Input.GetKey(key))
        {
            if (!keyStates[key])
            {
                ExecuteKeyFunction(key);
                keyStates[key] = true;
            }
        }
        else if (keyStates[key])
        {
            keyStates[key] = false;
        }
    }

    void ExecuteKeyFunction(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftShift:
                Debug.Log("Shift pressed");
                break;
            case KeyCode.Z:
                Debug.Log("Z pressed");
                TakeDamage(100);
                break;
        }
    }

    void HandleHealHold()
    {
        if (isHealing) return;

        if (Input.GetKey(KeyCode.H))
        {
            HealInventory healItem = inventory.healSlot; // Используем healSlot напрямую
            if (healItem != null)
            {
                ItemHeal itemHeal = inventory.data.items[healItem.id] as ItemHeal;
                if (itemHeal == null)
                {
                    Debug.LogError("Предмет не является ItemHeal.");
                    return;
                }

                float holdTime = 0.5F;
                healHoldTimer += Time.deltaTime;
                if (healHoldTimer >= holdTime)
                {
                    StartCoroutine(UseHealItem(healItem));
                    healHoldTimer = 0f;
                }
            }
        }
        else
        {
            healHoldTimer = 0f;
        }
    }


    private ItemHeal GetHealItem()
    {
        if (inventory != null && inventory.healSlot.id != 0)
        {
            return inventory.data.items[inventory.healSlot.id] as ItemHeal;
        }
        return null;
    }


    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        int finalDamage = damage;

        if (equippedArmor != null && armorSlot != null && armorSlot.currentDurability > 0)
        {
            int absorbedDamage = Mathf.RoundToInt(damage * equippedArmor.efficiency / 100f);
            absorbedDamage = Mathf.Min(absorbedDamage, armorSlot.currentDurability);

            armorSlot.currentDurability -= absorbedDamage;
            finalDamage = damage - absorbedDamage;

            Debug.Log($"Броня поглотила {absorbedDamage} урона. Осталось прочности: {armorSlot.currentDurability}/{armorSlot.maxDurability}");

            // Обновляем UI-слот брони через Inventory
            if (inventory != null)
            {
                inventory.UpdateArmorSlotText();
                inventory.UpdateInventory();      // перерисовываем именно слот брони
                // Если нужно обновить всю сетку items[i]:
                // inventory.UpdateInventoryEverything();
            }

            if (armorSlot.currentDurability <= 0)
            {
                armorSlot.currentDurability = 0;
                if (inventory != null)
                    inventory.OnArmorDestroyed();

                equippedArmor = null;
                armorSlot = null;
            }
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(HealInventory healItem)
    {
        if (isDead)
            return;
        int needHeal = maxHealth - currentHealth;
        if (needHeal > healItem.currentHeal) needHeal = healItem.currentHeal;
        currentHealth += needHeal;
        healItem.currentHeal -= needHeal;
        Debug.Log($"Healed +{needHeal}. Health: {currentHealth}/{maxHealth}");
    }

    public void EquipArmor(ItemArmor armor, ArmorInventory slot)
    {
        if (armor == null || slot == null)
            return;

        equippedArmor = armor;
        armorSlot = slot;

        Debug.Log($"Armor equipped: {armor.itemName} (Eff: {armor.efficiency}%, Dur: {armorSlot.currentDurability}/{armorSlot.maxDurability})");
    }

    public void RemoveArmor()
    {
        equippedArmor = null;
        armorSlot = null;
        Debug.Log("Armor removed");
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log("Player died!");

        // Показываем панель Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Останавливаем время (если надо заморозить игру)
        Time.timeScale = 0f;
    }

    // Этот метод привязан к кнопке «Начать заново»
    private void OnRestartButtonClicked()
    {
        // Снимаем паузу времени
        Time.timeScale = 1f;
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Этот метод привязан к кнопке «Выйти из игры»
    private void OnQuitButtonClicked()
    {
        // Если запущено в редакторе Unity, то просто остановит воспроизведение:
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // В собранном билде закроет приложение
        Application.Quit();
#endif
    }

    private IEnumerator UseHealItem(HealInventory healItem = null)
    {
        if (healItem == null)
            healItem = inventory.healSlot;

        if (healItem == null)
            yield break;

        isHealing = true;

        var movement = GetComponent<PlayerMovement2D>();
        if (movement != null)
            movement.canMove = false;

        Debug.Log("Начинаем использовать аптечку...");

        ItemHeal itemHeal = inventory.data.items[healItem.id] as ItemHeal;
        if (itemHeal == null)
        {
            Debug.LogError("Предмет не является типом ItemHeal!");
            yield break;
        }

        yield return new WaitForSeconds(itemHeal.healHoldTime);

        Heal(healItem);

        if (healItem.currentHeal <= 0)
        {
            if (healItem == inventory.healSlot)
            {
                inventory.OnHealDestroyed();
            }
        }
        else
        {
            if (healItem == inventory.healSlot)
            {
                inventory.UpdateHealSlotText();
            }
        }

        if (movement != null)
            movement.canMove = true;

        isHealing = false;
    }




}
