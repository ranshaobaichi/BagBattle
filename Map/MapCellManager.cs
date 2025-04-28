using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapCellManager : MonoBehaviour
{
    [System.Serializable]
    public class MapCellData
    {
        public int row;
        public int column;
        public bool isSelected;
        public MapCell.CellType cellType;
    }

    public static MapCellManager Instance { get; set; } // 单例
    public GameObject mapCellPrefab; // 地图格子预制体

    public const int mapRows = 5; // 地图格子行数
    public const int mapColumns = 9; // 地图格子列数
    public MapCell[,] mapCells; // 地图格子数组

    #region 初始位置及目标位置
    public const int walkLength = 2;
    private int playerPosX; // 玩家当前行
    private int playerPosY; // 玩家当前列

    public int startPosX; // 起始行
    public int startPosY; // 起始列

    public int targetPosX; // 目标行
    public int targetPosY; // 目标列
    #endregion

    [System.Serializable]
    public class MapData
    {
        public List<MapCellData> cells = new List<MapCellData>();
        public int playerPosX;
        public int playerPosY;
        public int startPosX;
        public int startPosY;
        public int targetPosX;
        public int targetPosY;
    }

    private const string MAP_DATA_FILENAME = "MapData";
    private const string MAP_DATA_PATH = "MapData";

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnEnable()
    {
        UpdateInteractableCells();
    }

    public void InitializeMapCells()
    {
        bool dataLoaded = false;
        if (mapCells == null || mapCells.Length == 0)
        {
            mapCells = new MapCell[mapRows, mapColumns]; // 初始化地图格子数组
        }

        // 初始化地图格子对象
        for (int row = 0; row < mapRows; row++)
        {
            for (int column = 0; column < mapColumns; column++)
            {
                GameObject cellObject = Instantiate(mapCellPrefab, transform); // 实例化地图格子
                MapCell mapCell = cellObject.GetComponent<MapCell>(); // 获取地图格子组件
                mapCell.Initialize(row, column); // 初始基础属性
                mapCells[row, column] = mapCell; // 将地图格子添加到数组中
            }
        }

        // 尝试从Resources加载数据
        TextAsset dataAsset = Resources.Load<TextAsset>(MAP_DATA_PATH);
        if (dataAsset != null && PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 0) // 如果找到数据且不是新游戏
        {
            try
            {
                MapData mapData = JsonUtility.FromJson<MapData>(dataAsset.text);

                foreach (MapCellData cellData in mapData.cells)
                {
                    if (cellData.row < mapRows && cellData.column < mapColumns)
                    {
                        MapCell mapCell = mapCells[cellData.row, cellData.column];
                        mapCell.Initialize(cellData.row, cellData.column, cellData.isSelected, cellData.cellType);
                    }
                }

                // 加载位置数据
                playerPosX = mapData.playerPosX;
                playerPosY = mapData.playerPosY;
                startPosX = mapData.startPosX;
                startPosY = mapData.startPosY;
                targetPosX = mapData.targetPosX;
                targetPosY = mapData.targetPosY;

                mapCells[playerPosX, playerPosY].Initialize(playerPosX, playerPosY, false, MapCell.CellType.Battle); // 设置玩家格子
                Debug.Log($"已从Resources加载地图数据，玩家位置: ({playerPosX}, {playerPosY})");
                Debug.Log($"玩家格子选取状态：{mapCells[playerPosX, playerPosY].IsSelected()}");

                dataLoaded = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("加载地图数据失败: " + e.Message);
            }
        }

        // 如果没有加载到数据，创建新文件
        if (!dataLoaded)
        {
            Debug.Log("未找到地图数据，创建新的地图数据");
            StoreMapCellData();
            mapCells[playerPosX, playerPosY].Initialize(playerPosX, playerPosY, false, MapCell.CellType.Battle); // 设置玩家格子
        }
        SetPlayerPosition(playerPosX, playerPosY); // 设置玩家位置
        UpdateInteractableCells(); // 更新可交互格子
    }

    public void StoreMapCellData()
    {
        MapData mapData = new MapData();

        // 添加格子数据
        for (int row = 0; row < mapRows; row++)
        {
            for (int column = 0; column < mapColumns; column++)
            {
                MapCell mapCell = mapCells[row, column];
                MapCellData cellData = new MapCellData
                {
                    row = mapCell.GetRow(),
                    column = mapCell.GetColumn(),
                    isSelected = mapCell.IsSelected(),
                    cellType = mapCell.GetCellType()
                };
                mapData.cells.Add(cellData);
            }
        }

        // 添加位置数据
        mapData.playerPosX = playerPosX;
        mapData.playerPosY = playerPosY;
        mapData.startPosX = startPosX;
        mapData.startPosY = startPosY;
        mapData.targetPosX = targetPosX;
        mapData.targetPosY = targetPosY;

        string json = JsonUtility.ToJson(mapData, true);

#if UNITY_EDITOR
        // 在编辑器模式下，可以将数据写入Resources文件夹
        string resourcesPath = "Assets/Resources";
        string directoryPath = Path.Combine(Application.dataPath, "Resources");

        // 确保Resources目录存在
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            AssetDatabase.Refresh();
        }

        // 保存JSON文件到Resources文件夹
        string filePath = Path.Combine(resourcesPath, MAP_DATA_FILENAME + ".json");
        File.WriteAllText(filePath, json);
        AssetDatabase.Refresh();
        Debug.Log("地图数据已保存到Resources: " + filePath);
#else
        Debug.Log("注意：运行时无法写入Resources文件夹。数据变更将不会被保存。");
#endif
    }

    public void LoadMapCellData()
    {
        TextAsset dataAsset = Resources.Load<TextAsset>(MAP_DATA_PATH);
        if (dataAsset != null)
        {
            MapData mapData = JsonUtility.FromJson<MapData>(dataAsset.text);

            // 加载格子数据
            foreach (MapCellData cellData in mapData.cells)
            {
                if (cellData.row < mapRows && cellData.column < mapColumns)
                {
                    MapCell mapCell = mapCells[cellData.row, cellData.column];
                    mapCell.Initialize(cellData.row, cellData.column, cellData.isSelected, cellData.cellType);
                }
            }

            // 加载位置数据
            playerPosX = mapData.playerPosX;
            playerPosY = mapData.playerPosY;
            startPosX = mapData.startPosX;
            startPosY = mapData.startPosY;
            targetPosX = mapData.targetPosX;
            targetPosY = mapData.targetPosY;

            Debug.Log($"已从Resources加载地图数据，玩家位置: ({playerPosX}, {playerPosY})");
        }
        else
        {
            Debug.LogWarning("在Resources文件夹中未找到地图数据");
        }
    }

    public void SetPlayerPosition(int x, int y)
    {
        if (x >= 0 && x < mapRows && y >= 0 && y < mapColumns)
        {
            for (int rowOffset = -walkLength; rowOffset <= walkLength; rowOffset++)
            {
                int currentRow = playerPosX + rowOffset;

                // 检查行是否在地图范围内
                if (currentRow < 0 || currentRow >= mapRows) continue;

                // 根据当前行偏移量，计算可能的列偏移范围
                int remainingSteps = walkLength - Mathf.Abs(rowOffset);

                // 在允许的列范围内寻找
                for (int colOffset = -remainingSteps; colOffset <= remainingSteps; colOffset++)
                {
                    int currentCol = playerPosY + colOffset;

                    // 检查列是否在地图范围内，且不是玩家当前位置
                    if (currentCol < 0 || currentCol >= mapColumns || currentCol < playerPosY) continue;
                    // if (rowOffset == 0 && colOffset == 0) continue; // 跳过玩家当前位置

                    // 曼哈顿距离刚好是 |rowOffset| + |colOffset|，已通过上面的逻辑保证不超过walkLength
                    mapCells[currentRow, currentCol].SetInteractable(false);
                }
            }
            mapCells[playerPosX, playerPosY].SetColor(new Color(1, 0, 0, 0.5f)); // 设置当前格子不可交互
            playerPosX = x;
            playerPosY = y;
            mapCells[playerPosX, playerPosY].SetColor(new Color(0.5f, 1, 0.4f, 0.4f)); // 设置玩家格子颜色
            Debug.Log($"设置玩家位置为: ({x}, {y})");
        }
        else
        {
            Debug.LogError($"无效的玩家位置: ({x}, {y})");
        }
    }
    public void UpdateInteractableCells()
    {
        mapCells[playerPosX, playerPosY].SetColor(new Color(0.5f, 1, 0.4f, 0.4f));  //绿色
        // 只检查玩家周围walkLength范围内的格子
        for (int rowOffset = -walkLength; rowOffset <= walkLength; rowOffset++)
        {
            int currentRow = playerPosX + rowOffset;

            // 检查行是否在地图范围内
            if (currentRow < 0 || currentRow >= mapRows) continue;

            // 根据当前行偏移量，计算可能的列偏移范围
            int remainingSteps = walkLength - Mathf.Abs(rowOffset);

            // 在允许的列范围内寻找
            for (int colOffset = -remainingSteps; colOffset <= remainingSteps; colOffset++)
            {
                int currentCol = playerPosY + colOffset;

                // 检查列是否在地图范围内，且不是玩家当前位置
                if (currentCol < 0 || currentCol >= mapColumns || currentCol < playerPosY) continue;
                if (currentCol == playerPosY && currentRow == playerPosX) continue; // 跳过玩家当前位置

                // 曼哈顿距离刚好是 |rowOffset| + |colOffset|，已通过上面的逻辑保证不超过walkLength
                mapCells[currentRow, currentCol].SetInteractable(true);
            }
        }
    }

    void OnDestroy()
    {
        StoreMapCellData();
    }
}