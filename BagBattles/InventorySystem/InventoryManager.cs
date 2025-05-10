using System;
using System.Collections.Generic;
using Assets.BagBattles.Types;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEngine.EventSystems;

[Serializable]
public class InventoryItemData
{
    public string guid; // 使用字符串存储GUID
    public Vector2 blankPos; // 空白区域物体位置
    public List<InventoryManager.GridPos> position; // 物品在网格上的位置
}

[Serializable]
public class InventoryManagerData
{
    [Serializable]
    public class TriggerInventoryItemData : InventoryItemData
    {
        public int functionType;
        public int specificType;
    }

    [Serializable]
    public class FoodInventoryItemData : InventoryItemData
    {
        public int foodType;
    }

    [Serializable]
    public class BulletInventoryItemData : InventoryItemData
    {
        public int bulletType;
    }

    [Serializable]
    public class SurroundInventoryItemData : InventoryItemData
    {
        public int surroundType;
    }

    [Serializable]
    public class OtherInventoryItemData : InventoryItemData
    {
        public int otherType;
    }
    public int rows, cols;
    public List<TriggerInventoryItemData> triggers = new List<TriggerInventoryItemData>();
    public List<FoodInventoryItemData> foods = new List<FoodInventoryItemData>();
    public List<BulletInventoryItemData> bullets = new List<BulletInventoryItemData>();
    public List<SurroundInventoryItemData> surrounds = new List<SurroundInventoryItemData>();
    public List<OtherInventoryItemData> others = new List<OtherInventoryItemData>();
}

public class InventoryManager : MonoBehaviour
{
    const string inventorySaveDataPath = "inventoryData.json";
    const string inventoryInitDataPath = "inventoryInitData";
    public static InventoryManager Instance { get; set; }
    [Serializable]
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
    [NonSerialized] public List<FoodInventoryItem> foodInInventory = new(); // 仓库中食物列表
    [NonSerialized] public List<BulletInventoryItem> bulletInInventory = new(); // 仓库中子弹列表
    [NonSerialized] public List<SurroundInventoryItem> surroundInInventory = new(); // 仓库中环绕物列表
    [NonSerialized] public List<OtherInventoryItem> otherInInventory = new(); // 仓库中环绕物列表

    public List<Transform> dropPoints = new(); // 物品掉落点列表
    #region 对外接口
    public int GetGridHeight() => rows; // 修正以获取网格高度
    public int GetGridWidth() => columns; // 修正以获取网格宽度
    public TriggerInventoryItem GetTriggerInventoryItemByGuid(Guid guid)
    {
        foreach (var triggerItem in triggerInInventory)
        {
            if (triggerItem.inventoryID == guid)
            {
                return triggerItem;
            }
        }
        Debug.LogError($"未找到GUID为{guid}的触发器道具");
        return null;
    } // 根据GUID获取触发器道具
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
        Debug.Log($"触发器添加到角色了{triggerInInventory.Count}个触发器道具");
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
    public GameObject DropItem(Item.ItemType itemType, object functionType, object specificType)
    {
        GameObject ret = null;
        if (itemType != Item.ItemType.TriggerItem)
        {
            Debug.LogError($"物品类型{itemType}错误调用触发器类加载器");
            return null;
        }
        if (functionType == null || specificType == null)
        {
            Debug.LogError($"物品类型{itemType}调用错误，functionType或specificType为空");
            return null;
        }

        Transform dropPoint = dropPoints != null ? dropPoints[UnityEngine.Random.Range(0, dropPoints.Count)] : inventoryPanel.transform;

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
                            return null;
                        }
                        // 生成物品并添加组件
                        ret = Instantiate(itemPrefab, dropPoint);
                        FireInventoryTriggerItem fireInventoryTriggerItem = ret.AddComponent<FireInventoryTriggerItem>();
                        if (!fireInventoryTriggerItem.Initialize(fireTriggerType))
                        {
                            Debug.LogError("FireInventoryTriggerItem initialization failed.");
                            Destroy(ret);
                            return null;
                        }
                        // 将生成物体加入管理列表中
                        triggerInInventory.Add(fireInventoryTriggerItem);
                    }
                    else
                    {
                        Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}下的具体类型{specificType}错误,无法获取触发器属性");
                        return null;
                    }
                    break;
                case Trigger.TriggerType.ByTime:
                    if (specificType is TimeTriggerType timeTriggerType)
                    {
                        Debug.Log("DropItem called with param: " + itemType + ", " + triggerType + ", " + timeTriggerType);
                        InventoryItem.ItemShape itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, timeTriggerType);
                        GameObject itemPrefab = GetGameobjectByShape(itemShape);
                        if (itemPrefab == null)
                        {
                            Debug.LogError("物品预制体未找到");
                            return null;
                        }
                        // 生成物品并添加组件
                        ret = Instantiate(itemPrefab, dropPoint);
                        TimeInventoryTriggerItem timeInventoryTriggerItem = ret.AddComponent<TimeInventoryTriggerItem>();
                        if (!timeInventoryTriggerItem.Initialize(timeTriggerType))
                        {
                            Debug.LogError("TimeInventoryTriggerItem initialization failed.");
                            Destroy(ret);
                            return null;
                        }
                        // 将生成物体加入管理列表中
                        triggerInInventory.Add(timeInventoryTriggerItem);
                    }
                    else
                    {
                        Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}下的具体类型{specificType}错误,无法获取触发器属性");
                    }
                    break;
                case Trigger.TriggerType.ByOtherTrigger:
                    if (specificType is ByOtherTriggerType otherTriggerType)
                    {
                        Debug.Log("DropItem called with param: " + itemType + ", " + triggerType + ", " + otherTriggerType);
                        InventoryItem.ItemShape itemShape = ItemAttribute.Instance.GetItemShape(itemType, triggerType, otherTriggerType);
                        GameObject itemPrefab = GetGameobjectByShape(itemShape);
                        if (itemPrefab == null)
                        {
                            Debug.LogError("物品预制体未找到");
                            return null;
                        }
                        // 生成物品并添加组件
                        ret = Instantiate(itemPrefab, dropPoint);
                        ByOtherInventoryTriggerItem otherInventoryTriggerItem = ret.AddComponent<ByOtherInventoryTriggerItem>();
                        if (!otherInventoryTriggerItem.Initialize(otherTriggerType))
                        {
                            Debug.LogError("ByOtherInventoryTriggerItem initialization failed.");
                            Destroy(ret);
                            return null;
                        }
                        // 将生成物体加入管理列表中
                        triggerInInventory.Add(otherInventoryTriggerItem);
                    }
                    else
                    {
                        Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}下的具体类型{specificType}错误,无法获取触发器属性");
                        return null;
                    }
                    break;
                default:
                    Debug.LogError($"触发器类型{(Trigger.TriggerType)functionType}错误或未实现,无法获取触发器属性");
                    return null;
            }
        }
        else
        {
            Debug.LogError($"未知触发器类型{functionType}");
            return null;
        }
        return ret;
    }

    /// <summary>
    /// 其他类型掉落
    /// </summary>
    /// <param name="itemType">基础类型</param>
    /// <param name="specificType">具体类型</param>
    public GameObject DropItem(Item.ItemType itemType, object specificType)
    {
        GameObject ret = null;

        Transform dropPoint = dropPoints != null ? dropPoints[UnityEngine.Random.Range(0, dropPoints.Count)] : inventoryPanel.transform;
        switch (itemType)
        {
            case Item.ItemType.BulletItem:
                // 获得形状
                if (specificType is not BulletType bulletType)
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹道具属性");
                    return null;
                }
                InventoryItem.ItemShape bulletItemShape = ItemAttribute.Instance.GetItemShape(itemType, bulletType);
                if (bulletItemShape == InventoryItem.ItemShape.NONE)
                {
                    Debug.LogError($"子弹类型{(BulletType)specificType}错误,无法获取子弹形状");
                    return null;
                }
                // 获得形状预制体
                GameObject bulletItemPrefab = GetGameobjectByShape(bulletItemShape);
                if (bulletItemPrefab == null)
                {
                    Debug.LogError("物品预制体未找到");
                    return null;
                }
                // 生成物品并添加组件
                ret = Instantiate(bulletItemPrefab, dropPoint);
                BulletInventoryItem bulletInventoryItem = ret.AddComponent<BulletInventoryItem>();
                if (!bulletInventoryItem.Initialize(bulletType))
                {
                    Debug.LogError("BulletInventoryItem initialization failed.");
                    Destroy(ret);
                    return null;
                }

                bulletInInventory.Add(bulletInventoryItem);
                break;
            case Item.ItemType.FoodItem:
                if (specificType is not FoodType foodType)
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物道具属性");
                    return null;
                }
                InventoryItem.ItemShape foodItemShape = ItemAttribute.Instance.GetItemShape(itemType, foodType);
                if (foodItemShape == InventoryItem.ItemShape.NONE)
                {
                    Debug.LogError($"食物类型{(FoodType)specificType}错误,无法获取食物形状");
                    return null;
                }
                // 获得形状预制体
                GameObject foodItemPrefab = GetGameobjectByShape(foodItemShape);
                if (foodItemPrefab == null)
                {
                    Debug.LogError("物品预制体未找到");
                    return null;
                }
                // 生成物品并添加组件
                ret = Instantiate(foodItemPrefab, dropPoint);
                FoodInventoryItem foodInventoryItem = ret.AddComponent<FoodInventoryItem>();
                if (!foodInventoryItem.Initialize(foodType))
                {
                    Debug.LogError("FoodInventoryItem initialization failed.");
                    Destroy(ret);
                    return null;
                }

                foodInInventory.Add(foodInventoryItem);
                break;
            case Item.ItemType.SurroundItem:
                if (specificType is not SurroundType surroundType)
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误,无法获取环绕物道具属性");
                    return null;
                }
                InventoryItem.ItemShape surroundItemShape = ItemAttribute.Instance.GetItemShape(itemType, surroundType);
                if (surroundItemShape == InventoryItem.ItemShape.NONE)
                {
                    Debug.LogError($"环绕物类型{(SurroundType)specificType}错误,无法获取环绕物形状");
                    return null;
                }
                // 获得形状预制体
                GameObject surroundItemPrefab = GetGameobjectByShape(surroundItemShape);
                if (surroundItemPrefab == null)
                {
                    Debug.LogError("物品预制体未找到");
                    return null;
                }
                // 生成物品并添加组件
                ret = Instantiate(surroundItemPrefab, dropPoint);
                SurroundInventoryItem surroundInventoryItem = ret.AddComponent<SurroundInventoryItem>();
                if (!surroundInventoryItem.Initialize(surroundType))
                {
                    Debug.LogError("SurroundInventoryItem initialization failed.");
                    Destroy(ret);
                    return null;
                }
                // 将生成物体加入管理列表中
                surroundInInventory.Add(surroundInventoryItem);
                break;
            case Item.ItemType.OtherItem:
                if (specificType is not OtherType otherType)
                {
                    Debug.LogError($"其他类别道具类型{(OtherType)specificType}错误,无法获取其他类别道具道具属性");
                    return null;
                }
                InventoryItem.ItemShape otherItemShape = ItemAttribute.Instance.GetItemShape(itemType, otherType);
                if (otherItemShape == InventoryItem.ItemShape.NONE)
                {
                    Debug.LogError($"其他类别道具类型{(OtherType)specificType}错误,无法获取其他类别道具形状");
                    return null;
                }
                // 获得形状预制体
                GameObject otherItemPrefab = GetGameobjectByShape(otherItemShape);
                if (otherItemPrefab == null)
                {
                    Debug.LogError("物品预制体未找到");
                    return null;
                }
                // 生成物品并添加组件
                ret = Instantiate(otherItemPrefab, dropPoint);
                OtherInventoryItem otherInventoryItem = ret.AddComponent<OtherInventoryItem>();
                if (!otherInventoryItem.Initialize(otherType))
                {
                    Debug.LogError("OtherInventoryItem initialization failed.");
                    Destroy(ret);
                    return null;
                }
                // 将生成物体加入管理列表中
                otherInInventory.Add(otherInventoryItem);
                break;
            default:
                Debug.LogError($"物品类型{itemType}错误或未实现,无法获取触发器属性");
                return null;
        }
        return ret;
    }
    public void RemoveFoodItem(FoodInventoryItem foodItem)
    {
        if (foodItem == null)
        {
            Debug.LogError("移除食物物品失败，物品为空");
            return;
        }

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
        Debug.Log("InventoryManager Awake");
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        inventoryPanel.GetComponent<GridLayoutGroup>().constraintCount = columns;
    }

    // 初始化背包网格
    void InitializeGrid()
    {
        if (rows == 0 || columns == 0)
        {
            Debug.LogError("hasn't set the col or row correctly!");
            return;
        }

        if (gridCells != null && gridCells.Count != 0)
        {
            Debug.Log("GridCells has been created, no need to create again.");
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
                centerContainer.transform.localScale = Vector3.one;
                gridCells[new GridPos(j, i)] = gridCell;
            }
        }
    }

    /// <summary>
    /// 尝试将物品放入格子中
    /// </summary>
    /// <param name="item">放置物品</param>
    /// <param name="targetCells">目标格子</param>
    /// <param name="target">返回的放置的格子</param>
    /// <returns>-2_Targetpos未初始化 -1_放置在空白区域  0_放置失败  1_放置成功</returns>
    public int TryPlaceItemInGrid(InventoryItem item, List<GridCell> targetCells, List<GridPos> target)
    {
        #region 错误检查
        if (target == null)
        {
            Debug.LogError("target is null");
            return -2;
        }
        if (item.GetShape() == InventoryItem.ItemShape.NONE)
        {
            Debug.LogError("物品形状错误");
            return -2;
        }
        if (item.GetDirection() == InventoryItem.Direction.NONE)
        {
            Debug.LogError("物品方向错误");
            return -2;
        }
        if (targetCells == null || targetCells.Count == 0)
        {
            Debug.Log("targetCells is null or empty, put on blank");
            return -1;
        }
        #endregion

        // 检测鼠标位置
        Debug.Log("targetCells数量: " + targetCells.Count);
        foreach (var cell in targetCells)
        {
            Debug.Log("cell pos: " + cell.gridPos.gridX + " " + cell.gridPos.gridY);
        }
        var mousePos = gameObject.activeInHierarchy ? GetCellIndex(Input.mousePosition) : targetCells[0].GetPos();
        // put on the blank area
        if (!gridCells.TryGetValue(mousePos, out _))
        {
            Debug.Log("put in blank area, mouse pos: " + mousePos.gridX + " " + mousePos.gridY);
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
            item.transform.SetParent(targetCell.centerContainer, false);

            // 设置物品位置在容器中心
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = Vector2.zero;

            item.gameObject.SetActive(true);
            Canvas itemCanvas = item.GetComponent<Canvas>();
            if (itemCanvas != null)
            {
                itemCanvas.overrideSorting = true;
                itemCanvas.sortingOrder = 1;
            }
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
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 查找带有GridCell组件的结果
        foreach (RaycastResult result in results)
        {
            GridCell gridCell = result.gameObject.GetComponent<GridCell>();
            if (gridCell != null)
            {
                return gridCell.gridPos;
            }
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

    public void StoreInventoryData()
    {
        InventoryManagerData data = new InventoryManagerData();
        data.rows = rows;
        data.cols = columns;

        // 保存触发器数据
        foreach (var item in triggerInInventory)
        {
            if (item == null) continue;

            InventoryManagerData.TriggerInventoryItemData itemData = new InventoryManagerData.TriggerInventoryItemData();
            itemData.guid = item.inventoryID.ToString();
            itemData.blankPos = item.GetComponent<RectTransform>().anchoredPosition;
            itemData.position = item.GetComponent<InventoryItem>()
                .GetCurrentLayOnGrid()
                .Select(gridPos => gridPos.gridPos)
                .ToList();
            // 根据触发器类型设置functionType和specificType
            switch (item.GetTriggerType())
            {
                case Trigger.TriggerType.ByFireTimes:
                    itemData.functionType = (int)Trigger.TriggerType.ByFireTimes;
                    itemData.specificType = (int)item.GetSpecificType();
                    break;
                case Trigger.TriggerType.ByTime:
                    itemData.functionType = (int)Trigger.TriggerType.ByTime;
                    itemData.specificType = (int)item.GetSpecificType();
                    break;
                case Trigger.TriggerType.ByOtherTrigger:
                    itemData.functionType = (int)Trigger.TriggerType.ByOtherTrigger;
                    itemData.specificType = (int)item.GetSpecificType();
                    break;
                default:
                    Debug.LogError($"未知触发器类型: {item.GetTriggerType()}");
                    break;
            }
            data.triggers.Add(itemData);
        }
        
        // 保存食物数据
        foreach (var item in foodInInventory)
        {
            if (item == null) continue;
            
            InventoryManagerData.FoodInventoryItemData itemData = new InventoryManagerData.FoodInventoryItemData();
            itemData.blankPos = item.GetComponent<RectTransform>().anchoredPosition;
            itemData.guid = item.inventoryID.ToString();
            itemData.position = item.GetComponent<InventoryItem>()
                .GetCurrentLayOnGrid()
                .Select(gridPos => gridPos.gridPos)
                .ToList();
            itemData.foodType = (int)item.GetSpecificType();
            
            data.foods.Add(itemData);
        }
        
        // 保存子弹数据
        foreach (var item in bulletInInventory)
        {
            if (item == null) continue;
            
            InventoryManagerData.BulletInventoryItemData itemData = new InventoryManagerData.BulletInventoryItemData();
            itemData.guid = item.inventoryID.ToString();
            itemData.blankPos = item.GetComponent<RectTransform>().anchoredPosition;
            itemData.position = item.GetComponent<InventoryItem>()
                .GetCurrentLayOnGrid()
                .Select(gridPos => gridPos.gridPos)
                .ToList();
            foreach(var gridPos in itemData.position)
            {
                Debug.Log($"子弹位置: {gridPos.gridX} {gridPos.gridY}");
            }
            itemData.bulletType = (int)item.GetSpecificType();
            
            data.bullets.Add(itemData);
        }

        // 保存环绕物数据
        foreach (var item in surroundInInventory)
        {
            if (item == null) continue;
            
            InventoryManagerData.SurroundInventoryItemData itemData = new InventoryManagerData.SurroundInventoryItemData();
            itemData.guid = item.inventoryID.ToString();
            itemData.blankPos = item.GetComponent<RectTransform>().anchoredPosition;
            itemData.position = item.GetComponent<InventoryItem>()
                .GetCurrentLayOnGrid()
                .Select(gridPos => gridPos.gridPos)
                .ToList();
            itemData.surroundType = (int)item.GetSpecificType();
            
            data.surrounds.Add(itemData);
        }

        // 保存其他类别道具数据
        foreach (var item in otherInInventory)
        {
            if (item == null) continue;
            
            InventoryManagerData.OtherInventoryItemData itemData = new InventoryManagerData.OtherInventoryItemData();
            itemData.guid = item.inventoryID.ToString();
            itemData.blankPos = item.GetComponent<RectTransform>().anchoredPosition;
            itemData.position = item.GetComponent<InventoryItem>()
                .GetCurrentLayOnGrid()
                .Select(gridPos => gridPos.gridPos)
                .ToList();
            itemData.otherType = (int)item.GetSpecificType();
            
            data.others.Add(itemData);
        }

        // 序列化并保存数据
        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(Application.persistentDataPath, inventorySaveDataPath);
        File.WriteAllText(filePath, json);
        
        Debug.Log($"背包数据已保存到: {filePath}");
    }

    public void LoadInventoryData()
    {
        // 初始化数据
        gridCells = new();
        triggerInInventory = new();
        foodInInventory = new();
        bulletInInventory = new();
        surroundInInventory = new();
        otherInInventory = new();

        string json = null;
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 1)
        {
            json = Resources.Load<TextAsset>(inventoryInitDataPath).text;
            Debug.Log($"从Resources加载背包数据: {inventoryInitDataPath}");
        }
        else
        {
            string filePath = Path.Combine(Application.persistentDataPath, inventorySaveDataPath);
            json = File.ReadAllText(filePath);
            Debug.Log($"从{filePath}加载背包数据");
        }
        if (json == null)
        {
            Debug.LogError("背包数据加载失败，数据为空或文件不存在。请检查路径和文件名。");
            return;
        }
        
        InventoryManagerData data = JsonUtility.FromJson<InventoryManagerData>(json);
        // 加载网格
        InitializeGrid(); 

        // 重新加载物品
        foreach (var itemData in data.triggers)
        {
            Trigger.TriggerType triggerType = (Trigger.TriggerType)itemData.functionType;
            GameObject item = null;
            // 根据触发器类型设置functionType和specificType
            // 生成触发器
            item = triggerType switch
            {
                Trigger.TriggerType.ByFireTimes => DropItem(Item.ItemType.TriggerItem, triggerType, (FireTriggerType)itemData.specificType),
                Trigger.TriggerType.ByTime => DropItem(Item.ItemType.TriggerItem, triggerType, (TimeTriggerType)itemData.specificType),
                Trigger.TriggerType.ByOtherTrigger => DropItem(Item.ItemType.TriggerItem, triggerType, (ByOtherTriggerType)itemData.specificType),
                _ => throw new System.ArgumentException($"未知触发器类型: {triggerType}")
            };

            // 设置位置等其他属性
            if (item != null)
            {
                InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
                inventoryItem.inventoryID = Guid.Parse(itemData.guid); // 设置GUID
                inventoryItem.triggerDectectFlag = true;
                if (itemData.position != null && itemData.position.Count > 0)
                    inventoryItem.TryPlaceItem(itemData.position);
                else
                    inventoryItem.GetComponent<RectTransform>().anchoredPosition = itemData.blankPos; // 设置空白区域物体位置
            }
            else
            {
                Debug.LogError($"无法加载触发器数据: {itemData.guid}");
                continue;
            }
        }

        foreach (var itemData in data.foods)
        {
            FoodType foodType = (FoodType)itemData.foodType;
            GameObject item = DropItem(Item.ItemType.FoodItem, foodType);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.inventoryID = Guid.Parse(itemData.guid); // 设置GUID
            inventoryItem.triggerDectectFlag = true;
            if (itemData.position != null && itemData.position.Count > 0)
                inventoryItem.TryPlaceItem(itemData.position);
            else
                inventoryItem.GetComponent<RectTransform>().anchoredPosition = itemData.blankPos; // 设置空白区域物体位置
        }
        
        foreach (var itemData in data.bullets)
        {
            BulletType bulletType = (BulletType)itemData.bulletType;
            GameObject item = DropItem(Item.ItemType.BulletItem, bulletType);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.inventoryID = Guid.Parse(itemData.guid);
            inventoryItem.triggerDectectFlag = true;
            if (itemData.position != null && itemData.position.Count > 0)
                inventoryItem.TryPlaceItem(itemData.position);
            else
                inventoryItem.GetComponent<RectTransform>().anchoredPosition = itemData.blankPos; // 设置空白区域物体位置
        }

        foreach (var itemData in data.surrounds)
        {
            SurroundType surroundType = (SurroundType)itemData.surroundType;
            GameObject item = DropItem(Item.ItemType.SurroundItem, surroundType);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.inventoryID = Guid.Parse(itemData.guid); // 设置GUID
            inventoryItem.triggerDectectFlag = true;
            if (itemData.position != null && itemData.position.Count > 0)
                inventoryItem.TryPlaceItem(itemData.position);
            else
                inventoryItem.GetComponent<RectTransform>().anchoredPosition = itemData.blankPos; // 设置空白区域物体位置
        }

        foreach (var itemData in data.others)
        {
            OtherType otherType = (OtherType)itemData.otherType;
            GameObject item = DropItem(Item.ItemType.OtherItem, otherType);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.inventoryID = Guid.Parse(itemData.guid); // 设置GUID
            inventoryItem.triggerDectectFlag = true;
            if (itemData.position != null && itemData.position.Count > 0)
                inventoryItem.TryPlaceItem(itemData.position);
            else
                inventoryItem.GetComponent<RectTransform>().anchoredPosition = itemData.blankPos; // 设置空白区域物体位置
        }
        // 触发器检测
        TriggerTriggerItem();
        Debug.Log($"背包数据已加载");
    }
}
