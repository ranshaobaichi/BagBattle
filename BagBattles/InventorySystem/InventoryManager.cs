using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; set; }
    [System.Serializable]
    public struct GridPos
    {
        public static int rows;
        public static int columns;
        public GridPos(int x, int y) => (gridX, gridY) = (x, y);
        public int gridX;
        public int gridY;
        /// <summary>
        /// 检查下标是否位于正确区间
        /// </summary>
        public bool OnGrid() { return gridX >= 0 && gridY >= 0 && gridX < rows && gridY < columns; }
    }

    // public List<Item.ItemType> DebugItemType = new();
    public GameObject gridCellPrefab; // GridCell预制体
    public GameObject inventoryItemPrefab; // 物品预制体
    public GameObject inventoryPanel;
    public GameObject InventorySystem;
    public int rows; // 网格行数
    public int columns; // 网格列数
    // private List<List<GridCell>> gridCells;
    private List<InventoryItem> existItem = new();
    private Dictionary<GridPos, GridCell> gridCells = new();// 网格字典
    private List<TriggerItem> triggerItems = new(); // 触发器列表

    public Item GetItemOnGridcell(GridPos pos) => gridCells[pos].itemOnGrid; // 获取格子上的物品类型
    public int GetGridHeight() => rows; // 修正以获取网格高度
    public int GetGridWidth() => columns; // 修正以获取网格宽度
    public void AddTriggerItem(TriggerItem item) => triggerItems.Add(item); // 添加触发器
    public void TriggerTriggerItem()
    {
        foreach (var item in triggerItems)
        {
            item.DetectItems();
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        GridPos.rows = rows;
        GridPos.columns = columns;

        // foreach (Item.ItemType type in DebugItemType)
        // {
        //     existItem.Add(CreateNewItem(type));
        // }
        inventoryPanel.GetComponent<GridLayoutGroup>().constraintCount = columns;
        InitializeGrid();
    }

    // 初始化背包网格
    void InitializeGrid()
    {
        if (rows == 0 || columns == 0)
        {
            Debug.LogError("hasn't set the col or row correctly!");
            return;
        }

        gridCells = new();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject gridCellObject = Instantiate(gridCellPrefab, inventoryPanel.transform);
                GridCell gridCell = gridCellObject.GetComponent<GridCell>();
                gridCell.Initialize(new GridPos(j, i));
                gridCells[new GridPos(j, i)] = gridCell;
            }
        }
    }

    // 尝试将物品放入格子中
    public bool TryPlaceItemInGrid(InventoryItem item, List<GridCell> targetCells)
    {
        var mousePos = GetCellIndex(Input.mousePosition);
        //put on the blank area
        if (!gridCells.TryGetValue(mousePos, out _) && targetCells.Count == 0)
        {
            item.transform.SetParent(InventorySystem.transform);
            // Debug.Log("put on blank");
            return true;
        }

        List<GridPos> target = PlaceItem(mousePos, item.GetShape(), targetCells);
        if (target != null && target.Count != 0)
        {
            foreach (var cell in target)
            {
                // Debug.Log("cell pos: " + cell.gridX + " " + cell.gridY);
                gridCells[cell].SetCanPlaceItem(false);
                gridCells[cell].itemOnGrid = item.item; // 设置格子物品类型
                item.LayOnGrid(gridCells[cell]);
            }
            item.transform.SetParent(gridCells[target[0]].transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            // item.transform.position = targetCells[0].transform.position;
            return true;
        }

        return false;
    }

    private List<GridPos> PlaceItem(GridPos mousePos, InventoryItem.Shape Shape, List<GridCell> targetCells)
    {
        // Debug.Log(targetCells.Count);
        if (!(gridCells.TryGetValue(mousePos, out GridCell mouseCell) && mouseCell.GetCanPlace()))
            return null;
        switch (Shape.itemShape)
        {
            case InventoryItem.ItemShape.SQUARE_11:
                return new List<GridPos> { mousePos };
            case InventoryItem.ItemShape.RECT_12:  //假设抓住上半部分
                if (targetCells.Count < 2) return null;
                return PlaceRect12(mousePos, Shape);
            case InventoryItem.ItemShape.RECT_13:
                if (targetCells.Count < 3) return null;
                return PlaceRect13(mousePos, Shape);
            case InventoryItem.ItemShape.L_SHAPE_12_11:
                if (targetCells.Count < 3) return null;
                return PlaceL_12_11(mousePos, Shape);
        }
        return null;
    }

    // 根据位置获取格子的索引
    private GridPos GetCellIndex(Vector3 position)
    {
        foreach (var (gridCellPos, gridCell) in gridCells)
            if (RectTransformUtility.RectangleContainsScreenPoint(gridCell.GetComponent<RectTransform>(), position))
            {
                return gridCellPos;
            }
        return new GridPos(-1, -1); // 没有找到可放置的格子
    }

    // 创建一个新物品
    // public InventoryItem CreateNewItem(Item.ItemType type)
    // {
    //     GameObject itemObject = Instantiate(inventoryItemPrefab, transform.parent);
    //     InventoryItem item = itemObject.GetComponent<InventoryItem>();
    //     item.SetItemDetails(type);
    //     return item;
    // }

    private List<GridPos> PlaceRect12(GridPos mousePos, InventoryItem.Shape shape)
    {
        switch (shape.itemDirection)
        {
            case InventoryItem.Direction.UP:
                GridPos downCell = new(mousePos.gridX, mousePos.gridY + 1);
                if (gridCells.TryGetValue(downCell, out GridCell mouseCell1) && mouseCell1.GetCanPlace())
                    return new List<GridPos> { mousePos, downCell };
                break;
            case InventoryItem.Direction.DOWN:
                GridPos upCell = new(mousePos.gridX, mousePos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell mouseCell2) && mouseCell2.GetCanPlace())
                    return new List<GridPos> { mousePos, upCell };
                break;
            case InventoryItem.Direction.LEFT:
                GridPos rightCell = new(mousePos.gridX + 1, mousePos.gridY);
                if (gridCells.TryGetValue(rightCell, out GridCell mouseCell3) && mouseCell3.GetCanPlace())
                    return new List<GridPos> { mousePos, rightCell };
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos leftCell = new(mousePos.gridX - 1, mousePos.gridY);
                if (gridCells.TryGetValue(leftCell, out GridCell mouseCell4) && mouseCell4.GetCanPlace())
                    return new List<GridPos> { mousePos, leftCell };
                break;
        }
        return null;
    }

    private List<GridPos> PlaceRect13(GridPos mousePos, InventoryItem.Shape shape)
    {
        switch (shape.itemDirection)
        {
            case InventoryItem.Direction.UP:
                GridPos downCell = new(mousePos.gridX, mousePos.gridY + 1);
                Debug.Log(downCell.gridX + " " + downCell.gridY);
                if (gridCells.TryGetValue(downCell, out GridCell mouseCell5) && mouseCell5.GetCanPlace())
                {
                    GridPos downCell2 = new(mousePos.gridX, mousePos.gridY - 1);
                    Debug.Log(downCell2.gridX + " " + downCell2.gridY);
                    if (gridCells.TryGetValue(downCell2, out GridCell mouseCell6) && mouseCell6.GetCanPlace())
                        return new List<GridPos> { mousePos, downCell, downCell2 };
                }
                break;
            case InventoryItem.Direction.DOWN:
                GridPos upCell = new(mousePos.gridX, mousePos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell mouseCell7) && mouseCell7.GetCanPlace())
                {
                    GridPos upCell2 = new(mousePos.gridX, mousePos.gridY + 1);
                    if (gridCells.TryGetValue(upCell2, out GridCell mouseCell8) && mouseCell8.GetCanPlace())
                        return new List<GridPos> { mousePos, upCell, upCell2 };
                }
                break;
            case InventoryItem.Direction.LEFT:
                GridPos rightCell = new(mousePos.gridX + 1, mousePos.gridY);
                if (gridCells.TryGetValue(rightCell, out GridCell mouseCell9) && mouseCell9.GetCanPlace())
                {
                    GridPos rightCell2 = new(mousePos.gridX - 1, mousePos.gridY);
                    if (gridCells.TryGetValue(rightCell2, out GridCell mouseCell10) && mouseCell10.GetCanPlace())
                        return new List<GridPos> { mousePos, rightCell, rightCell2 };
                }
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos leftCell = new(mousePos.gridX - 1, mousePos.gridY);
                if (gridCells.TryGetValue(leftCell, out GridCell mouseCel11) && mouseCel11.GetCanPlace())
                {
                    GridPos leftCell2 = new(mousePos.gridX + 1, mousePos.gridY);
                    if (gridCells.TryGetValue(leftCell2, out GridCell mouseCell12) && mouseCell12.GetCanPlace())
                        return new List<GridPos> { mousePos, leftCell, leftCell2 };
                }
                break;
        }
        return null;
    }

    private List<GridPos> PlaceL_12_11(GridPos mousePos, InventoryItem.Shape shape)
    {
        switch (shape.itemDirection)
        {
            case InventoryItem.Direction.UP:
                GridPos upCell = new(mousePos.gridX, mousePos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell mouseCell1) && mouseCell1.GetCanPlace())
                {
                    GridPos rightCell = new(mousePos.gridX + 1, mousePos.gridY);
                    if (gridCells.TryGetValue(rightCell, out GridCell mouseCell2) && mouseCell2.GetCanPlace())
                        return new List<GridPos> { mousePos, upCell, rightCell };
                }
                break;
            case InventoryItem.Direction.DOWN:
                GridPos downCell = new(mousePos.gridX, mousePos.gridY + 1);
                if (gridCells.TryGetValue(downCell, out GridCell mouseCell3) && mouseCell3.GetCanPlace())
                {
                    GridPos leftCell1 = new(mousePos.gridX - 1, mousePos.gridY);
                    if (gridCells.TryGetValue(leftCell1, out GridCell mouseCell4) && mouseCell4.GetCanPlace())
                        return new List<GridPos> { mousePos, downCell, leftCell1 };
                }
                break;
            case InventoryItem.Direction.LEFT:
                GridPos leftCell2 = new(mousePos.gridX - 1, mousePos.gridY);
                if (gridCells.TryGetValue(leftCell2, out GridCell mouseCell5) && mouseCell5.GetCanPlace())
                {
                    GridPos upCell2 = new(mousePos.gridX, mousePos.gridY - 1);
                    if (gridCells.TryGetValue(upCell2, out GridCell mouseCell6) && mouseCell6.GetCanPlace())
                        return new List<GridPos> { mousePos, leftCell2, upCell2 };
                }
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos rightCell3 = new(mousePos.gridX + 1, mousePos.gridY);
                if (gridCells.TryGetValue(rightCell3, out GridCell mouseCell7) && mouseCell7.GetCanPlace())
                {
                    GridPos downCell2 = new(mousePos.gridX, mousePos.gridY + 1);
                    if (gridCells.TryGetValue(downCell2, out GridCell mouseCell8) && mouseCell8.GetCanPlace())
                        return new List<GridPos> { mousePos, rightCell3, downCell2 };
                }
                break;
        }
        return null;
    }
}
