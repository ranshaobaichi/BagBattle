using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MapCell : MonoBehaviour
{
    public enum CellType
    {
        Battle,
        Shop,
    }

    public Image image; // 地图格子图片
    public Button button; // 地图格子按钮  

    private int cellRow; // 行
    private int cellColumn; // 列
    private bool canBeSelected = true; // 是否可以被选取 -- 记录是否走过
    public CellType cellType; // 地图格子类型

    // 添加访问器方法
    public int GetRow() => cellRow;
    public int GetColumn() => cellColumn;
    public bool IsSelected() => canBeSelected;
    public CellType GetCellType() => cellType;
    public void SetInteractable(bool flag) => button.interactable = flag && canBeSelected; // 设置按钮是否可交互
    public void SetColor(Color color) => image.color = color;

    public void Initialize(int row, int column, bool canBeSelected = true, CellType cellType = CellType.Battle)
    {
        cellRow = row;
        cellColumn = column;
        this.canBeSelected = canBeSelected;
        this.cellType = cellType;
        if (canBeSelected == false)
        {
            button.interactable = false; // 设置按钮不可交互
            image.color = new Color(1, 0, 0, 0.5f); // 设置为红色半透明
        }
    }

    public void SelectCell()
    {
        canBeSelected = false;
        image.color = new Color(1, 0, 0, 0.5f); // 设置为红色半透明
        MapCellManager.Instance.SetPlayerPosition(cellRow, cellColumn); // 设置玩家位置
        SwitchScene();
    }

    public void SwitchScene()
    {
        switch (cellType)
        {
            case CellType.Battle:
                BackToBattleScene();
                break;
            case CellType.Shop:
                break;
            default:
                break;
        }
        StoreGame();
        MapCellManager.Instance.gameObject.SetActive(false); // 隐藏地图
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

    public void StoreGame()
    {
        InventoryManager.Instance.StoreInventoryData();
        PlayerController.Instance.StorePlayerData();
        BulletSpawner.Instance.StoreBulletData();
        HealthController.Instance.StoreHealthData();
        MapCellManager.Instance.StoreMapCellData();
    }
}
