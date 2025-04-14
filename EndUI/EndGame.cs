using UnityEngine;

public class EndGame : MonoBehaviour
{
    public void BackToEnterScene()
    {
        DestroyAllPersistentSingletons();
        UnityEngine.SceneManagement.SceneManager.LoadScene("EnterScene");
        PlayerPrefs.SetInt(PlayerPrefsKeys.HAS_REMAINING_GAME_KEY, 0); // 重置剩余游戏次数
        PlayerPrefs.Save();
    }

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