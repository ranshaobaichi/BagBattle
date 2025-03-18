using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryTriggerItem : Item
{
    #region 组件属性
    [Header("绑定物品")]
    // [SerializeField] public Trigger.BaseTriggerAttribute triggerItemAttribute;
    public InventoryItem inventoryItem;
    public Dictionary<ItemType, List<object>> triggerItems = new Dictionary<ItemType, List<object>>(); //记录该触发器可触发的物品
    #endregion

    public override void UseItem() { throw new NotImplementedException(); }
    public abstract Trigger.TriggerType GetTriggerType();

    private bool HasSpace(InventoryManager.GridPos gridPos, InventoryItem.Direction direction)
    {
        int gridHeight = InventoryManager.Instance.GetGridHeight();
        int gridWidth = InventoryManager.Instance.GetGridWidth();
        switch (direction)
        {
            case InventoryItem.Direction.UP:
                if (gridPos.gridY + 1 < gridHeight)
                    return true;
                break;
            case InventoryItem.Direction.DOWN:
                if (gridPos.gridY - 1 >= 0)
                    return true;
                break;
            case InventoryItem.Direction.LEFT:
                if (gridPos.gridX - 1 >= 0)
                    return true;
                break;
            case InventoryItem.Direction.RIGHT:
                if (gridPos.gridX + 1 < gridWidth)
                    return true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return false;
    }

    //TODO: 改为从哪格开始触发 触发方向 触发格数
    public void DetectItems()
    {
        List<Item> ContainItems = new List<Item>(); // 触发器检测到的物品
        triggerItems.Clear();
        // 触发逻辑
        switch (((Trigger.BaseTriggerAttribute)GetAttribute()).triggerRange)
        {
            case Trigger.TriggerRange.SingleCell:
                ContainItems.AddRange(DetectStraightDirection(inventoryItem.GetShape().itemDirection, 1));
                break;
            case Trigger.TriggerRange.DoubleCell:
                ContainItems.AddRange(DetectStraightDirection(inventoryItem.GetShape().itemDirection, 2));
                break;
            case Trigger.TriggerRange.TripleCell:
                ContainItems.AddRange(DetectStraightDirection(inventoryItem.GetShape().itemDirection, 3));
                break;
            case Trigger.TriggerRange.FullRow:
                ContainItems.AddRange(DetectStraightDirection(inventoryItem.GetShape().itemDirection, InventoryManager.Instance.GetGridWidth() + 3));
                break;
            case Trigger.TriggerRange.FourStraightSingleCell:
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.UP, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.DOWN, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.LEFT, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.RIGHT, 1));
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
                triggerItems.Add(item.GetItemType(), new List<object>());
            if(item.GetAttribute() != null)
                triggerItems[item.GetItemType()].Add(item.GetAttribute());
            else
                Debug.LogError($"触发器检测到的物品类型{item.GetItemType()}不支持GetAttribute方法");
            item.triggerDectectFlag = true; // 触发器检测到物品后，设置该物品不可被触发器检测
        }

        // 触发器工作完成
        Debug.Log("触发器工作完成，检测到物品数量：" + ContainItems.Count);
    }

    private List<Item> DetectStraightDirection(InventoryItem.Direction direction, int cellCount)
    {
        Debug.Log($"触发器开始工作，当前方向：{direction}，触发格数：{cellCount}");
        InventoryManager.GridPos basePos = inventoryItem.currentLayOnGrid[0].gridPos;
        List<Item> ContainItems = new List<Item>();
        // 处理触发方向和单元格数量的逻辑
        switch (direction)
        {
            case InventoryItem.Direction.UP:
                // 处理向上触发
                foreach (var pos in inventoryItem.currentLayOnGrid)
                {
                    if (pos.gridPos.gridY < basePos.gridY)
                        basePos = pos.gridPos;
                }
                basePos.gridY--;    // 刨除当前格子
                // 找到能触发的所有物品
                while (basePos.gridY >= 0 && cellCount > 0)
                {
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridY--;
                    cellCount--;
                }
                break;
            case InventoryItem.Direction.DOWN:
                // 处理向下触发
                foreach (var pos in inventoryItem.currentLayOnGrid)
                {
                    if (pos.gridPos.gridY > basePos.gridY)
                        basePos = pos.gridPos;
                }
                basePos.gridY++;    // 刨除当前格子
                while (basePos.gridY < InventoryManager.Instance.GetGridHeight() && cellCount > 0)
                {
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridY++;
                    cellCount--;
                }
                break;
            case InventoryItem.Direction.LEFT:
                // 处理向左触发
                foreach (var pos in inventoryItem.currentLayOnGrid)
                {
                    if (pos.gridPos.gridX < basePos.gridX)
                        basePos = pos.gridPos;
                }
                basePos.gridX--;    // 刨除当前格子
                while (basePos.gridX >= 0 && cellCount > 0)
                {
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                    basePos.gridX--;
                    cellCount--;
                }
                break;
            case InventoryItem.Direction.RIGHT:
                foreach (var pos in inventoryItem.currentLayOnGrid)
                {
                    if (pos.gridPos.gridX > basePos.gridX)
                        basePos = pos.gridPos;
                }
                basePos.gridX++;    // 刨除当前格子
                while (basePos.gridX < InventoryManager.Instance.GetGridWidth() && cellCount > 0)
                {
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
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

    public List<Item> DetectSpecial(Trigger.TriggerRange triggerRange)
    {
        Debug.Log($"触发器开始工作，当前方向：{triggerRange}");
        InventoryManager.GridPos basePos = inventoryItem.currentLayOnGrid[0].gridPos;
        List<Item> ContainItems = new List<Item>();

        switch (triggerRange)
        {
            case Trigger.TriggerRange.NineGrid:
                bool upFlag = HasSpace(basePos, InventoryItem.Direction.UP), 
                    downFlag = HasSpace(basePos, InventoryItem.Direction.DOWN),
                    leftFlag = HasSpace(basePos, InventoryItem.Direction.LEFT),
                    rightFlag = HasSpace(basePos, InventoryItem.Direction.RIGHT);
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.UP, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.DOWN, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.LEFT, 1));
                ContainItems.AddRange(DetectStraightDirection(InventoryItem.Direction.RIGHT, 1));
                InventoryManager.GridPos tmpPos = basePos;
                if (upFlag && leftFlag)
                {
                    tmpPos.gridX = basePos.gridX - 1;
                    tmpPos.gridY = basePos.gridY - 1;
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (upFlag && rightFlag)
                {
                    tmpPos.gridX = basePos.gridX + 1;
                    tmpPos.gridY = basePos.gridY - 1;
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (downFlag && leftFlag)
                {
                    tmpPos.gridX = basePos.gridX - 1;
                    tmpPos.gridY = basePos.gridY + 1;
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
                        ContainItems.Add(tmp);
                        tmp.triggerDectectFlag = false; // 触发器检测到物品后，设置该物品不可被触发器检测
                    }
                }
                if (downFlag && rightFlag)
                {
                    tmpPos.gridX = basePos.gridX + 1;
                    tmpPos.gridY = basePos.gridY + 1;
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(tmpPos);
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && tmp.triggerDectectFlag == true)
                    {
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
