using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    private const string MAP_DATA_PATH = "MapData";
    private const string MAP_DATA_FILENAME = "MapData";

    public Button newGameButton;
    public Button continueGameButton;
    public Button quitGameButton;

    #region 按钮事件
    public void Start()
    {
        // 设置按钮事件
        newGameButton.onClick.AddListener(NewGame);
        continueGameButton.onClick.AddListener(ContinueGame);
        quitGameButton.onClick.AddListener(QuitGame);
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.HAS_REMAINING_GAME_KEY) == 0)
            continueGameButton.interactable = false; // 如果没有剩余游戏，禁用继续游戏按钮
        else
            continueGameButton.interactable = true; // 否则启用继续游戏按钮
    }

    public void NewGame()
    {
        // 查找并销毁所有DontDestroyOnLoad的单例对象
        PlayerPrefs.SetInt(PlayerPrefsKeys.NEW_GAME_KEY, 1); // 1 represents true
        PlayerPrefs.Save(); // 保存PlayerPrefs
        // 加载地图场景
        SceneManager.LoadScene("StartLoadingScene");
    }
    public void ContinueGame()
    {
        // 加载地图场景
        PlayerPrefs.SetInt(PlayerPrefsKeys.NEW_GAME_KEY, 0); // 0 represents false
        PlayerPrefs.Save(); // 保存PlayerPrefs
        SceneManager.LoadScene("StartLoadingScene");
    }
    public void QuitGame()
    {
        // 退出游戏
        PlayerPrefs.Save(); // 保存PlayerPrefs
        Application.Quit();
        // 在编辑器中停止播放模式
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 在构建版本中退出应用
        Application.Quit();
        #endif
    }
#endregion
}
