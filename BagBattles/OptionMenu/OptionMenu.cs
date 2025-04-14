using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionMenu : MonoBehaviour
{
    public GameObject optionButton; // 选项菜单对象
    private void StoreOptions()
    {
        // Store options here, e.g., volume, graphics settings, etc.
        // This is just a placeholder for the actual implementation.
    }
    #region 按钮事件
    public void QuitGame()
    {
        StoreOptions();
        PlayerPrefs.SetString(PlayerPrefsKeys.CURRENT_SCENE_KEY, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        TimeController.Instance.ResumeGame();
        // 隐藏所有DontDestroyOnLoad的单例对象
        DestroyAllPersistentSingletons();
        SceneManager.LoadScene("EnterScene");
    }
    public void CloseMenu()
    {
        StoreOptions();
        optionButton.SetActive(true); // 显示选项按钮
        if (optionButton.GetComponent<OptionButton>().previousInventoryUIActive)
            InventorySystem.Instance.gameObject.SetActive(true);
        TimeController.Instance.ResumeGame();
        gameObject.SetActive(false);
    }
    #endregion
    private void DestroyAllPersistentSingletons()
    {
        // 销毁所有可能使用了DontDestroyOnLoad的单例
        if (InventorySystem.Instance != null)
            Destroy(InventorySystem.Instance.gameObject);

        if (InventoryManager.Instance != null)
            Destroy(InventoryManager.Instance.gameObject);

        if (PlayerController.Instance != null)
            Destroy(PlayerController.Instance.gameObject);

        if (WaveManager.Instance != null)
            Destroy(WaveManager.Instance.gameObject);

        if (TimeController.Instance != null)
            Destroy(TimeController.Instance.gameObject);

        // 查找任何其他可能存在的DontDestroyOnLoad对象并销毁
        GameObject[] persistentObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in persistentObjects)
        {
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                Destroy(obj);
            }
        }
    }
}
