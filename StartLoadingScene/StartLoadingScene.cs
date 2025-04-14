using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartLoadingScene : MonoBehaviour
{
    // private const string CURRENT_WAVE_KEY = "CurrentWaveIndex";
    public GameObject inventorySystemPrefab;
    public GameObject playerControllerPrefab;
    public GameObject timeControllerPrefab;
    public GameObject waveSystemPrefab;
    public GameObject mapPrefab;

    void Start()
    {
        StartCoroutine(InitializeAndLoad());
    }

    IEnumerator InitializeAndLoad()
    {
        // 初始化管理器并标记为DontDestroyOnLoad
        if (InventoryManager.Instance == null)
        {
            GameObject invSystem = Instantiate(inventorySystemPrefab);
            DontDestroyOnLoad(invSystem);
            invSystem.name = inventorySystemPrefab.name; // 设置实例名称
            InventorySystem.Instance = invSystem.GetComponent<InventorySystem>();
            InventoryManager.Instance = invSystem.GetComponentInChildren<InventoryManager>();
        }

        if (PlayerController.Instance == null)
        {
            GameObject playerCtrl = Instantiate(playerControllerPrefab);
            DontDestroyOnLoad(playerCtrl);
            playerCtrl.name = playerControllerPrefab.name;
            PlayerController.Instance = playerCtrl.GetComponent<PlayerController>();
            HealthController.Instance = playerCtrl.GetComponentInChildren<HealthController>();
            BulletSpawner.Instance = playerCtrl.GetComponentInChildren<BulletSpawner>();
        }

        if (TimeController.Instance == null)
        {
            GameObject timeCtrl = Instantiate(timeControllerPrefab);
            DontDestroyOnLoad(timeCtrl);
            timeCtrl.name = timeControllerPrefab.name;
            TimeController.Instance = timeCtrl.GetComponent<TimeController>();
        }

        if (WaveManager.Instance == null)
        {
            GameObject waveSys = Instantiate(waveSystemPrefab);
            DontDestroyOnLoad(waveSys);
            WaveManager.Instance = waveSys.GetComponentInChildren<WaveManager>();
            waveSys.name = waveSystemPrefab.name;
        }

        if (MapCellManager.Instance == null)
        {
            GameObject map = Instantiate(mapPrefab);
            DontDestroyOnLoad(map);
            MapCellManager.Instance = map.GetComponentInChildren<MapCellManager>();
            map.name = mapPrefab.name;
            MapCellManager.Instance.InitializeMapCells();
        }

        Debug.Log("All managers initialized and marked as DontDestroyOnLoad.");
        // 等待初始化完成
        yield return new WaitForSeconds(.5f);
        PlayerPrefs.SetInt(PlayerPrefsKeys.HAS_REMAINING_GAME_KEY, 1);
        InventoryManager.Instance.LoadInventoryData();

        if (PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 0)
        {
            Debug.Log("New game started, initializing data.");
            // 继续游戏，加载数据
            if (InventoryManager.Instance == null || PlayerController.Instance == null)
            {
                Debug.LogError("InventoryManager or PlayerController is null, loading data failed.");
            }
            else
            {
                // 继续游戏，加载数据
                PlayerController.Instance.LoadPlayerData();
                HealthController.Instance.LoadHealthData();
                BulletSpawner.Instance.LoadBulletData();
                switch (PlayerPrefs.GetString(PlayerPrefsKeys.CURRENT_SCENE_KEY))
                {
                    case "BattleScene":
                        BackToBattleScene();
                        break;
                    case "Map":
                        SceneManager.LoadScene("Map");
                        break;
                    default:
                        Debug.LogError($"Scene {PlayerPrefs.GetString(PlayerPrefsKeys.CURRENT_SCENE_KEY)} not found!");
                        break;
                }
            }
        }
        else
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.CURRENT_WAVE_KEY, 0);
            PlayerPrefs.Save();
            MapCellManager.Instance.transform.parent.gameObject.SetActive(true);
            SceneManager.LoadScene("Map");
        }
        Debug.Log("Game data loaded.");
    }

    private void BackToBattleScene()
    {
        PlayerController.Instance.SetActive(true);
        TimeController.Instance.SetActive(true);

        // 查找非活跃的WaveSystem
        GameObject waveSystem = null;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "WaveSystem")
            {
                waveSystem = obj;
                break;
            }
        }

        if (waveSystem != null)
            waveSystem.SetActive(true);
        else
            Debug.LogError("WaveSystem not found!");

        // InventorySystem.Instance.SetActive(false);
        WaveManager.Instance.SetActive(true);
        SceneManager.LoadScene("BattleScene");
    }
}
