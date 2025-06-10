using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public Inventory playerInventory;
    public List<Crate> crates = new List<Crate>();

    private string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        var saveData = new GameSaveData
        {
            playerInventory = playerInventory.GetSaveData(),
            saveDate = System.DateTime.Now.ToString()
        };

        // Сохраняем позицию и скорость игрока
        var player = playerInventory.GetComponent<PlayerMovement2D>();
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                saveData.playerPosition[0] = rb.position.x;
                saveData.playerPosition[1] = rb.position.y;
                saveData.playerPosition[2] = player.transform.position.z;
                saveData.playerVelocity[0] = rb.linearVelocity.x;
                saveData.playerVelocity[1] = rb.linearVelocity.y;
            }
        }

        // Сохраняем данные ящиков
        foreach (var crate in crates)
        {
            if (crate == null) continue;

            var crateSave = new CrateSaveData
            {
                crateId = crate.gameObject.name
            };

            // Получаем данные инвентаря ящика
            var crateInventory = crate.GetInventoryData();
            if (crateInventory != null)
            {
                crateSave.items = crateInventory.items;
            }

            saveData.crates.Add(crateSave);
        }

        // Сохраняем в файл
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Игра сохранена в {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Файл сохранения не найден!");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<GameSaveData>(json);

        // Загружаем инвентарь игрока
        if (playerInventory != null)
        {
            playerInventory.LoadFromSaveData(saveData.playerInventory);
        }

        // Загружаем позицию и скорость игрока
        var player = playerInventory.GetComponent<PlayerMovement2D>();
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = new Vector2(saveData.playerPosition[0], saveData.playerPosition[1]);
                player.transform.position = new Vector3(saveData.playerPosition[0], saveData.playerPosition[1], saveData.playerPosition[2]);
                rb.linearVelocity = new Vector2(saveData.playerVelocity[0], saveData.playerVelocity[1]);
            }
        }

        // Загружаем данные ящиков
        foreach (var crateSave in saveData.crates)
        {
            var crate = crates.Find(c => c != null && c.gameObject.name == crateSave.crateId);
            if (crate != null)
            {
                var crateInventory = new InventorySaveData { items = crateSave.items };
                crate.LoadFromSaveData(crateInventory);
            }
        }

        // После загрузки состояния игрока
        var shooting = playerInventory.GetComponent<Shooting>();
        if (shooting != null)
        {
            shooting.ResetFireState();
        }

        Debug.Log("Игра загружена!");
    }

    public void RegisterCrate(Crate crate)
    {
        if (!crates.Contains(crate))
        {
            crates.Add(crate);
        }
    }

    public void UnregisterCrate(Crate crate)
    {
        crates.Remove(crate);
    }
} 