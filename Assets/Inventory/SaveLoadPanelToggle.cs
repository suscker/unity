using UnityEngine;

public class SaveLoadPanelToggle : MonoBehaviour
{
    public GameObject saveLoadPanel; 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveLoadPanel != null)
            {
                bool isActive = !saveLoadPanel.activeSelf;
                saveLoadPanel.SetActive(isActive);
                Time.timeScale = isActive ? 0f : 1f;
            }
        }
    }
}
