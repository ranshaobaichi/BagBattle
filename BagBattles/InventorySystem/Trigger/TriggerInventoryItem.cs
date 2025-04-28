using System;
using System.Collections.Generic;
using UnityEngine;

// BUG: 添加triggerPoint，用来规定触发器的触发点
public abstract class TriggerInventoryItem : InventoryItem
{
    #region 组件属性
    [Header("绑定物品")]
    // [SerializeField] public Trigger.BaseTriggerAttribute triggerItemAttribute;
    public Dictionary<Item.ItemType, List<(InventoryItem inventorySource, object specificType)>> triggerItems; //记录该触发器可触发的物品
    protected Trigger.TriggerType triggerType;
    protected Trigger.TriggerRange triggerRange;
    #endregion

    #region 对外接口
    public Trigger.TriggerType GetTriggerType() => triggerType;
    public Trigger.TriggerRange GetTriggerRange() => triggerRange;
    #endregion

    public TriggerInventoryItem() => itemType = Item.ItemType.TriggerItem;
    private bool HasSpace(InventoryManager.GridPos gridPos, Direction direction)
    {
        int gridHeight = InventoryManager.Instance.GetGridHeight();
        int gridWidth = InventoryManager.Instance.GetGridWidth();
        switch (direction)
        {
            case Direction.UP:
                if (gridPos.gridY + 1 < gridHeight)
                    return true;
                break;
            case Direction.DOWN:
                if (gridPos.gridY - 1 >= 0)
                    return true;
                break;
            case Direction.LEFT:
                if (gridPos.gridX - 1 >= 0)
                    return true;
                break;
            case Direction.RIGHT:
                if (gridPos.gridX + 1 < gridWidth)
                    return true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return false;
    }
    protected new void Awake()
    {
        base.Awake();
        triggerItems = new Dictionary<Item.ItemType, List<(InventoryItem inventorySource, object specificType)>>();
    }


    //TODO: 改为从哪格开始触发 触发方向 触发格数
    public bool DetectItems()
    {
        if (currentLayOnGrid == null || currentLayOnGrid.Count == 0)
        {
            Debug.Log("触发器未放置在格子上");
            return false;
        }
        List<InventoryItem> ContainItems = new List<InventoryItem>(); // 触发器检测到的物品
        triggerItems = new();
        // 触发逻辑
        switch (triggerRange)
        {
            case Trigger.TriggerRange.SingleCell:
                ContainItems.AddRange(DetectStraightDirection(itemDirection, 1));
                break;
            case Trigger.TriggerRange.DoubleCell:
                ContainItems.AddRange(DetectStraightDirection(itemDirection, 2));
                break;
            case Trigger.TriggerRange.TripleCell:
                ContainItems.AddRange(DetectStraightDirection(itemDirection, 3));
                break;
            case Trigger.TriggerRange.FullRow:
                ContainItems.AddRange(DetectStraightDirection(itemDirection, InventoryManager.Instance.GetGridWidth() + 3));
                break;
            case Trigger.TriggerRange.FourStraightSingleCell:
                ContainItems.AddRange(DetectStraightDirection(Direction.UP, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.DOWN, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.LEFT, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.RIGHT, 1));
                break;
            // case Trigger.TriggerRange.FourBiasSingleCell:
            //     break;
            case Trigger.TriggerRange.NineGrid:
                ContainItems.AddRange(DetectSpecial(Trigger.TriggerRange.NineGrid));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var item in ContainItems)
        {
            if (triggerItems.ContainsKey(item.GetItemType()) == false)
                triggerItems.Add(item.GetItemType(), new List<(InventoryItem inventorySource, object specificType)>());
            if (item.GetItemType() != Item.ItemType.None)
            {
                triggerItems[item.GetItemType()].Add((item, item.GetSpecificType()));
                Debug.Log("触发器检测到物品：" + item.GetItemType() + " " + item.GetSpecificType());
            }
            else
                Debug.LogError($"未绑定抽象物品类型");
            item.triggerDectectFlag = true;
        }

        // 触发器工作完成
        Debug.Log("触发器工作完成，检测到物品数量：" + ContainItems.Count);
        return true;
    }

    private List<InventoryItem> DetectStraightDirection(Direction direction, int cellCount)
    {
        Debug.Log($"触发器开始工作，当前方向：{direction}，触发格数：{cellCount}");
        InventoryManager.GridPos basePos = currentLayOnGrid[0].gridPos;
        List<InventoryItem> ContainItems = new List<InventoryItem>();
        // 处理触发方向和单元格数量的逻辑
        switch (direction)
        {
            case Direction.UP:
                // 处理向上触发
                foreach (var pos in currentLayOnGrid)
                {
                    if (pos.gridPos.gridY < basePos.gridY)
                        basePos = pos.gridPos;
                }
                basePos.gridY--;    // 刨除当前格子
                // 找到能触发的所有物品
                while (basePos.gridY >= 0 && cellCount > 0)
                {
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    else
                    {
                        Debug.Log($"检测到({basePos.gridX},{basePos.gridY})物品但是由于为null或触发标志为false");
                    }
                    basePos.gridY--;
                    cellCount--;
                }
                break;
            case Direction.DOWN:
                // 处理向下触发
                foreach (var pos in currentLayOnGrid)
                {
                    if (pos.gridPos.gridY > basePos.gridY)
                        basePos = pos.gridPos;
                }
                basePos.gridY++;    // 刨除当前格子
                while (basePos.gridY < InventoryManager.Instance.GetGridHeight() && cellCount > 0)
                {
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridY++;
                    cellCount--;
                }
                break;
            case Direction.LEFT:
                // 处理向左触发
                foreach (var pos in currentLayOnGrid)
                {
                    if (pos.gridPos.gridX < basePos.gridX)
                        basePos = pos.gridPos;
                }
                basePos.gridX--;    // 刨除当前格子
                while (basePos.gridX >= 0 && cellCount > 0)
                {
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridX--;
                    cellCount--;
                }
                break;
            case Direction.RIGHT:
                foreach (var pos in currentLayOnGrid)
                {
                    if (pos.gridPos.gridX > basePos.gridX)
                        basePos = pos.gridPos;
                }
                basePos.gridX++;    // 刨除当前格子
                while (basePos.gridX < InventoryManager.Instance.GetGridWidth() && cellCount > 0)
                {
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridX++;
                    cellCount--;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return ContainItems;
    }

    public List<InventoryItem> DetectSpecial(Trigger.TriggerRange triggerRange)
    {
        Debug.Log($"触发器开始工作，当前方向：{triggerRange}");
        InventoryManager.GridPos basePos = currentLayOnGrid[0].gridPos;
        List<InventoryItem> ContainItems = new List<InventoryItem>();

        switch (triggerRange)
        {
            case Trigger.TriggerRange.NineGrid:
                bool upFlag = HasSpace(basePos, Direction.UP),
                    downFlag = HasSpace(basePos, Direction.DOWN),
                    leftFlag = HasSpace(basePos, Direction.LEFT),
                    rightFlag = HasSpace(basePos, Direction.RIGHT);
                ContainItems.AddRange(DetectStraightDirection(Direction.UP, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.DOWN, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.LEFT, 1));
                ContainItems.AddRange(DetectStraightDirection(Direction.RIGHT, 1));
                InventoryManager.GridPos tmpPos = basePos;
                if (upFlag && leftFlag)
                {
                    tmpPos.gridX = basePos.gridX - 1;
                    tmpPos.gridY = basePos.gridY - 1;
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (upFlag && rightFlag)
                {
                    tmpPos.gridX = basePos.gridX + 1;
                    tmpPos.gridY = basePos.gridY - 1;
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (downFlag && leftFlag)
                {
                    tmpPos.gridX = basePos.gridX - 1;
                    tmpPos.gridY = basePos.gridY + 1;
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (downFlag && rightFlag)
                {
                    tmpPos.gridX = basePos.gridX + 1;
                    tmpPos.gridY = basePos.gridY + 1;
                    InventoryItem tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.triggerDectectFlag == true)
                    {
                        if (tmp.GetItemType() != Item.ItemType.None)
                            Debug.Log($"触发器检测到物品：{tmp.GetItemType()}");
                        else
                        {
                            Debug.LogError($"触发器检测到的物品类型为None");
                            throw new ArgumentOutOfRangeException();
                        }
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return ContainItems;
    }
}
