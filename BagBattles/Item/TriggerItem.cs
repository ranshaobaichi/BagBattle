using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItem : Item
{
    [Serializable]
    [Tooltip("触发范围")]
    public enum TriggerRange
    {
        [Tooltip("单格")] SingleCell,
        [Tooltip("双格")] DoubleCell,
    }

    // 触发类型枚举
    [Serializable]
    [Tooltip("触发方式")]
    public enum TriggerType
    {
        [Tooltip("按时间触发")] ByTime,
        [Tooltip("按开火次数触发")] ByFireTimes,
    }

    [SerializeField] private TriggerRange triggerRange;
    [SerializeField] private TriggerType triggerType;

    // 按时间触发的配置
    [SerializeField] private float triggerTime;

    // 按开火次数触发的配置
    [SerializeField] private int triggerFireCount;

    public InventoryItem inventoryItem;
    public List<Item> triggerItems = new(); //记录该触发器可触发的物品

    public override void UseItem() => DetectItems();

    public void DetectItems()
    {
        triggerItems.Clear();
        // 触发逻辑
        switch (triggerRange)
        {
            case TriggerRange.SingleCell:
                TriggerDirection(inventoryItem.GetShape().itemDirection, 1);
                break;
            case TriggerRange.DoubleCell:
                TriggerDirection(inventoryItem.GetShape().itemDirection, 2);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private HashSet<Item> TriggerDirection(InventoryItem.Direction direction, int cellCount)
    {
        InventoryManager.GridPos basePos = inventoryItem.currentLayOnGrid[0].gridPos;
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
                while (basePos.gridY >= 0 && cellCount > 0)
                {
                    Item tmp = InventoryManager.Instance.GetItemOnGridcell(basePos);
                    if (tmp != null && tmp.GetItemType() != ItemType.TriggerItem && !triggerItems.Contains(tmp))
                        triggerItems.Add(InventoryManager.Instance.GetItemOnGridcell(basePos));
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
                    if (tmp.GetItemType() != Item.ItemType.TriggerItem && !triggerItems.Contains(tmp))
                        triggerItems.Add(tmp);
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
                    if (tmp.GetItemType() != Item.ItemType.TriggerItem && !triggerItems.Contains(tmp))
                        triggerItems.Add(tmp);
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
                    if (tmp.GetItemType() != Item.ItemType.TriggerItem && !triggerItems.Contains(tmp))
                        triggerItems.Add(tmp);
                    basePos.gridX++;
                    cellCount--;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return null;
    }
}
