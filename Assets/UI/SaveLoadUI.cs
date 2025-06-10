using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI References")]
    public Button saveButton;
    public Button loadButton;
    public TMP_Text statusText;

    private void Start()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveGame);
        }

        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadGame);
        }

        UpdateStatusText("Готов к сохранению/загрузке");
    }

    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            UpdateStatusText("Игра сохранена!");
        }
        else
        {
            UpdateStatusText("Ошибка: SaveManager не найден!");
        }
    }

    public void LoadGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            UpdateStatusText("Игра загружена!");
        }
        else
        {
            UpdateStatusText("Ошибка: SaveManager не найден!");
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
} 