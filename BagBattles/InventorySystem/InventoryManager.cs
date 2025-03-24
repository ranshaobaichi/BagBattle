using System;
using System.Collections.Generic;
using System.Data;
using Assets.BagBattles.Types;
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
    [NonSerialized] public Dictionary<GridPos, GridCell> gridCells;// 网格字典
    [SerializeField] public List<TriggerInventoryItem> triggerInInventory = new(); // 仓库中触发器列表
    [NonSerialized] public List<FoodInventoryItem> foodInInventory; // 仓库中食物列表
    [NonSerialized] public List<BulletInventoryItem> bulletInInventory; // 仓库中子弹列表

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
            Debug.Log($"触发器当前方向：{triggerItem.GetDirection()}，触发格数：{triggerItem.GetTriggerRange()}");
            if (triggerItem.DetectItems())
                PlayerController.Instance.AddTriggerItem(triggerItem);
        }
    }
    /// <summary>
    /// 掉落触发器类型
    /// </summary>
    /// <param name="itemType">基础类型</param>
    /// <param name="functionType">功能类型</param>
    /// <param name="specificType">具体类型</param>
    public void DropItem(Item.ItemType itemType, object functionType, object specificType)
    {
        if (itemType != Item.ItemType.TriggerItem)
        {
            Debug.LogError($"物品类型{itemType}错误调用触发器类加载器");
            return;
        }
        if(functionType == null || specificType == null)
        {
            Debug.LogError($"物品类型{itemType}调用错误，functionType或specificType为空");
            return;
        }
        if (functionType is Trigger.TriggerType triggerType)
        {
            switch (triggerType)
            {
                case Trigger.TriggerType.ByFireTimes:
                    if (specificType is FireTriggerType fireTriggerType)
                    {
                        Debug.Log("DropItem called with param: " + itemType + ", " + triggerType + ", " + fireTriggerType);
                        // TODO:生成位置
                        InventoryItem.ItemShape itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, fireTriggerType);
                        GameObject itemPrefab = GetGameobjectByShape(itemShape);
                        if (itemPrefab == null)
                        {
                            Debug.LogError("物品预制体未找到");
                            return;
                        }
                        // 生成物品并添加组件
                        GameObject fireTrigger = Instantiate(itemPrefab, InventorySystem.transform);
                        FireInventoryTriggerItem fireInventoryTriggerItem = fireTrigger.AddComponent<FireInventoryTriggerItem>();
                        if (!fireInventoryTriggerItem.Initialize(fireTriggerType))
                        {
                            Debug.LogError("FireInventoryTriggerItem initialization failed.");
                            Destroy(fireTrigger);
                            return;
                        }
                        // 将生成物体加入管理列表中
                        triggerInInventory.Add(fireInventoryTriggerItem);
                    }
                    else
                    {
                        Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}下的具体类型{specificType}错误,无法获取触发器属性");
                    }
                    break;
                case Trigger.TriggerType.ByTime:
                    if (specificType is TimeTriggerType timeTriggerType)
                    {
                        Debug.Log("DropItem called with param: " + itemType + ", " + triggerType + ", " + timeTriggerType);
                        // TODO:生成位置
                        InventoryItem.ItemShape itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, timeTriggerType);
                        GameObject itemPrefab = GetGameobjectByShape(itemShape);
                        if (itemPrefab == null)
                        {
                            Debug.LogError("物品预制体未找到");
                            return;
                        }
                        // 生成物品并添加组件
                        GameObject timeTrigger = Instantiate(itemPrefab, InventorySystem.transform);
                        TimeInventoryTriggerItem timeInventoryTriggerItem = timeTrigger.AddComponent<TimeInventoryTriggerItem>();
                        if (!timeInventoryTriggerItem.Initialize(timeTriggerType))
                        {
                            Debug.LogError("TimeInventoryTriggerItem initialization failed.");
                            Destroy(timeTrigger);
                            return;
                        }
                        // 将生成物体加入管理列表中
                        triggerInInventory.Add(timeInventoryTriggerItem);
                    }
                    else
                    {
                        Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}下的具体类型{specificType}错误,无法获取触发器属性");
                    }
                    break;
                default:
                    Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}错误或未实现,无法获取触发器属性");
                    return;
            }
        }
        else
        {
            Debug.LogError($"未知触发器类型{functionType}");
            return;
        }
    }

    /// <summary>
    /// 其他类型掉落
    /// </summary>
    /// <param name="itemType">基础类型</param>
    /// <param name="specificType">具体类型</param>
    public void DropItem(Item.ItemType itemType, object specificType)
    {
        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                // 获得形状
                if(specificType is not BulletType bulletType)
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹道具属性");
                    return;
                }
                InventoryItem.ItemShape itemShape = ItemAttribute.Instance.GetItemShape(itemType, bulletType);
                if(itemShape == InventoryItem.ItemShape.NONE)
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹形状");
                    return;
                }
                // 获得形状预制体
                GameObject itemPrefab = GetGameobjectByShape(itemShape);
                if (itemPrefab == null)
                {
                    Debug.LogError("物品预制体未找到");
                    return;
                }
                // 生成物品并添加组件
                GameObject bulletItem = Instantiate(itemPrefab, InventorySystem.transform);
                BulletInventoryItem bulletInventoryItem = bulletItem.AddComponent<BulletInventoryItem>();
                if(!bulletInventoryItem.Initialize(bulletType))
                {
                    Debug.LogError("BulletInventoryItem initialization failed.");
                    Destroy(bulletItem);
                    return;
                }

                bulletInInventory.Add(bulletInventoryItem);
                break;
            case Item.ItemType.FoodItem:
                break;
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取触发器属性");
                return;
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

        // 初始化变量    
        gridCells = new();
        triggerInInventory = new();
        foodInInventory = new();
        bulletInInventory = new();

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
                
                // 创建一个中心点容器
                GameObject centerContainer = new GameObject("CenterContainer");
                centerContainer.transform.SetParent(gridCellObject.transform);
                RectTransform centerRect = centerContainer.AddComponent<RectTransform>();
                centerRect.anchorMin = Vector2.zero;
                centerRect.anchorMax = Vector2.one;
                centerRect.offsetMin = Vector2.zero;
                centerRect.offsetMax = Vector2.zero;
                
                GridCell gridCell = gridCellObject.GetComponent<GridCell>();
                gridCell.Initialize(new GridPos(j, i));
                gridCell.centerContainer = centerContainer.transform; // 需要在GridCell类中添加此字段
                gridCells[new GridPos(j, i)] = gridCell;
            }
        }
    }

    // 尝试将物品放入格子中
    // return : -2_Targetpos未初始化 -1_放置在空白区域  0_放置失败  1_放置成功
    public int TryPlaceItemInGrid(InventoryItem item, List<GridCell> targetCells, List<GridPos> target)
    {
        #region 错误检查
        if (target == null)
        {
            Debug.LogError("target is null");
            return -2;
        }
        if(item.GetShape() == InventoryItem.ItemShape.NONE)
        {
            Debug.LogError("物品形状错误");
            return -2;
        }
        if(item.GetDirection() == InventoryItem.Direction.NONE)
        {
            Debug.LogError("物品方向错误");
            return -2;
        }
        #endregion
        // 检测鼠标位置
        var mousePos = GetCellIndex(Input.mousePosition);
        //put on the blank area
        if (!gridCells.TryGetValue(mousePos, out _))
        {
            item.transform.SetParent(InventorySystem.transform);
            return -1;
        }

        // 放置物体
        var result = PlaceItem(mousePos, item.GetShape(), item.GetDirection(), targetCells);
        if (result != null && result.Count != 0)
        {
            target.Clear();
            target.AddRange(result);
            GridCell targetCell = gridCells[result[0]];
            
            // 将物品放在中心容器下而不是直接放在格子下
            item.transform.SetParent(targetCell.centerContainer);
            
            // 设置物品位置在容器中心
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = Vector2.zero;
            
            return 1;
        }
        else    // 放置失败
            return 0;
    }

    private List<GridPos> PlaceItem(GridPos startPos, InventoryItem.ItemShape itemShape, InventoryItem.Direction direction, List<GridCell> targetCells)
    {
        // Debug.Log(targetCells.Count);
        if (!(gridCells.TryGetValue(startPos, out GridCell startCell) && startCell.GetCanPlace()))
            return null;
        switch (itemShape)
        {
            case InventoryItem.ItemShape.SQUARE_11:
                return new List<GridPos> { startPos };
            case InventoryItem.ItemShape.RECT_12:  //假设抓住上半部分
                if (targetCells.Count < 2) return null;
                return PlaceRect12(startPos, direction);
            case InventoryItem.ItemShape.RECT_13:
                if (targetCells.Count < 3) return null;
                return PlaceRect13(startPos, direction);
            case InventoryItem.ItemShape.L_12_11:
                if (targetCells.Count < 3) return null;
                return PlaceL_12_11(startPos, direction);
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

    private List<GridPos> PlaceRect12(GridPos startPos, InventoryItem.Direction direction)
    {
        switch (direction)
        {
            case InventoryItem.Direction.UP:
                GridPos downCell = new(startPos.gridX, startPos.gridY + 1);
                if (gridCells.TryGetValue(downCell, out GridCell startCell1) && startCell1.GetCanPlace())
                    return new List<GridPos> { startPos, downCell };
                break;
            case InventoryItem.Direction.DOWN:
                GridPos upCell = new(startPos.gridX, startPos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell startCell2) && startCell2.GetCanPlace())
                    return new List<GridPos> { startPos, upCell };
                break;
            case InventoryItem.Direction.LEFT:
                GridPos rightCell = new(startPos.gridX + 1, startPos.gridY);
                if (gridCells.TryGetValue(rightCell, out GridCell startCell3) && startCell3.GetCanPlace())
                    return new List<GridPos> { startPos, rightCell };
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos leftCell = new(startPos.gridX - 1, startPos.gridY);
                if (gridCells.TryGetValue(leftCell, out GridCell startCell4) && startCell4.GetCanPlace())
                    return new List<GridPos> { startPos, leftCell };
                break;
        }
        return null;
    }
    private List<GridPos> PlaceRect13(GridPos startPos, InventoryItem.Direction direction)
    {
        switch (direction)
        {
            case InventoryItem.Direction.UP:
                GridPos downCell = new(startPos.gridX, startPos.gridY + 1);
                Debug.Log(downCell.gridX + " " + downCell.gridY);
                if (gridCells.TryGetValue(downCell, out GridCell startCell5) && startCell5.GetCanPlace())
                {
                    GridPos downCell2 = new(startPos.gridX, startPos.gridY - 1);
                    Debug.Log(downCell2.gridX + " " + downCell2.gridY);
                    if (gridCells.TryGetValue(downCell2, out GridCell startCell6) && startCell6.GetCanPlace())
                        return new List<GridPos> { startPos, downCell, downCell2 };
                }
                break;
            case InventoryItem.Direction.DOWN:
                GridPos upCell = new(startPos.gridX, startPos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell startCell7) && startCell7.GetCanPlace())
                {
                    GridPos upCell2 = new(startPos.gridX, startPos.gridY + 1);
                    if (gridCells.TryGetValue(upCell2, out GridCell startCell8) && startCell8.GetCanPlace())
                        return new List<GridPos> { startPos, upCell, upCell2 };
                }
                break;
            case InventoryItem.Direction.LEFT:
                GridPos rightCell = new(startPos.gridX + 1, startPos.gridY);
                if (gridCells.TryGetValue(rightCell, out GridCell startCell9) && startCell9.GetCanPlace())
                {
                    GridPos rightCell2 = new(startPos.gridX - 1, startPos.gridY);
                    if (gridCells.TryGetValue(rightCell2, out GridCell startCell10) && startCell10.GetCanPlace())
                        return new List<GridPos> { startPos, rightCell, rightCell2 };
                }
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos leftCell = new(startPos.gridX - 1, startPos.gridY);
                if (gridCells.TryGetValue(leftCell, out GridCell startCel11) && startCel11.GetCanPlace())
                {
                    GridPos leftCell2 = new(startPos.gridX + 1, startPos.gridY);
                    if (gridCells.TryGetValue(leftCell2, out GridCell startCell12) && startCell12.GetCanPlace())
                        return new List<GridPos> { startPos, leftCell, leftCell2 };
                }
                break;
        }
        return null;
    }
    private List<GridPos> PlaceL_12_11(GridPos startPos, InventoryItem.Direction direction)
    {
        switch (direction)
        {
            case InventoryItem.Direction.UP:
                GridPos upCell = new(startPos.gridX, startPos.gridY - 1);
                if (gridCells.TryGetValue(upCell, out GridCell startCell1) && startCell1.GetCanPlace())
                {
                    GridPos rightCell = new(startPos.gridX + 1, startPos.gridY);
                    if (gridCells.TryGetValue(rightCell, out GridCell startCell2) && startCell2.GetCanPlace())
                        return new List<GridPos> { startPos, upCell, rightCell };
                }
                break;
            case InventoryItem.Direction.DOWN:
                GridPos downCell = new(startPos.gridX, startPos.gridY + 1);
                if (gridCells.TryGetValue(downCell, out GridCell startCell3) && startCell3.GetCanPlace())
                {
                    GridPos leftCell1 = new(startPos.gridX - 1, startPos.gridY);
                    if (gridCells.TryGetValue(leftCell1, out GridCell startCell4) && startCell4.GetCanPlace())
                        return new List<GridPos> { startPos, downCell, leftCell1 };
                }
                break;
            case InventoryItem.Direction.LEFT:
                GridPos leftCell2 = new(startPos.gridX - 1, startPos.gridY);
                if (gridCells.TryGetValue(leftCell2, out GridCell startCell5) && startCell5.GetCanPlace())
                {
                    GridPos upCell2 = new(startPos.gridX, startPos.gridY - 1);
                    if (gridCells.TryGetValue(upCell2, out GridCell startCell6) && startCell6.GetCanPlace())
                        return new List<GridPos> { startPos, leftCell2, upCell2 };
                }
                break;
            case InventoryItem.Direction.RIGHT:
                GridPos rightCell3 = new(startPos.gridX + 1, startPos.gridY);
                if (gridCells.TryGetValue(rightCell3, out GridCell startCell7) && startCell7.GetCanPlace())
                {
                    GridPos downCell2 = new(startPos.gridX, startPos.gridY + 1);
                    if (gridCells.TryGetValue(downCell2, out GridCell startCell8) && startCell8.GetCanPlace())
                        return new List<GridPos> { startPos, rightCell3, downCell2 };
                }
                break;
        }
        return null;
    }

    private GameObject GetGameobjectByShape(InventoryItem.ItemShape itemShape)
    {
        string path = "InventoryItem/" + itemShape.ToString();
        GameObject itemPrefab = Resources.Load<GameObject>(path);
        if (itemPrefab != null)
        {
            return itemPrefab;
        }
        else
        {
            Debug.LogError($"未找到物品预制体：{path}");
        }
        return null;
    }
}
