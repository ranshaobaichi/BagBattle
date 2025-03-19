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
    public Dictionary<GridPos, GridCell> gridCells = new();// 网格字典
    public List<TriggerInventoryItem> triggerInInventory = new(); // 仓库中触发器列表
    public List<FoodInventoryItem> foodInInventory = new(); // 仓库中食物列表

    #region 对外接口

    public int GetGridHeight() => rows; // 修正以获取网格高度
    public int GetGridWidth() => columns; // 修正以获取网格宽度
    public InventoryItem GetItemOnGridcell(GridPos pos)
    {
        if (gridCells.TryGetValue(pos, out GridCell gridCell))
        {
            return gridCell.itemOnGrid;
        }
        else
        {
            Debug.Log($"格子超出范围：{pos.gridX} {pos.gridY}");
            return null;
        }
    } // 获取格子上的物品类型
    // 获取触发器及其可触发的物体
    // 并将触发器添加到角色
    public void TriggerTriggerItem()
    {
        foreach (var triggerItem in triggerInInventory)
        {
            var shape = triggerItem.GetShape();
            Debug.Log($"触发器当前方向：{shape.itemDirection}，触发格数：{((Trigger.BaseTriggerAttribute)triggerItem.GetAttribute()).triggerRange}");
            if (triggerItem.DetectItems())
                PlayerController.Instance.AddTriggerItem(triggerItem, triggerItem.GetTriggerType());
        }
    }

    public void AddToInventory(InventoryItem item)
    {
        if (item is TriggerInventoryItem triggerItem)
        {
            triggerInInventory.Add(triggerItem);
            Debug.Log($"添加触发器物品成功，当前数量：{triggerInInventory.Count}");
        }
        else if (item is FoodInventoryItem foodItem)
        {
            foodInInventory.Add(foodItem);
            Debug.Log($"添加食物物品成功，当前数量：{foodInInventory.Count}");
        }
        else
        {
            Debug.LogError("添加物品失败，物品类型不支持");
        }
    }
    
    public void RemoveFoodItem(FoodInventoryItem foodItem)
    {
        if (foodInInventory.Contains(foodItem))
        {
            foodInInventory.Remove(foodItem);
            Destroy(foodItem.gameObject); // 销毁物品对象
            Debug.Log($"移除食物物品成功，当前数量：{foodInInventory.Count}");
        }
        else
        {
            Debug.LogError("移除食物物品失败，物品不在仓库中");
        }
    }
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        GridPos.rows = rows;
        GridPos.columns = columns;
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
    // return : -2_Targetpos未初始化 -1_放置在空白区域  0_放置失败  1_放置成功
    public int TryPlaceItemInGrid(InventoryItem item, List<GridCell> targetCells, List<GridPos> target)
    {
        if (target == null)
        {
            Debug.LogError("target is null");
            return -2;
        }

        var mousePos = GetCellIndex(Input.mousePosition);
        //put on the blank area
        if (!gridCells.TryGetValue(mousePos, out _))
        {
            item.transform.SetParent(InventorySystem.transform);
            return -1;
        }

        var result = PlaceItem(mousePos, item.GetShape(), targetCells);
        if (result != null && result.Count != 0)
        {
            target.Clear();
            target.AddRange(result);
            item.transform.SetParent(gridCells[result[0]].transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return 1;
        }
        else    // 放置失败
            return 0;
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
