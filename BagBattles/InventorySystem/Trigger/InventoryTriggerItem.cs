using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTriggerItem : MonoBehaviour
{
    #region 组件属性
    [Header("触发器属性")]
    [SerializeField] public Trigger.BaseTriggerAttribute triggerItemAttribute;
    public InventoryItem inventoryItem;
    public Dictionary<Item.ItemType, List<object>> triggerItems = new(); //记录该触发器可触发的物品
    #endregion

    //FIXME: 改为从哪格开始触发 触发方向 触发格数
    public void DetectItems()
    {
        triggerItems.Clear();
        // 触发逻辑
        switch (triggerItemAttribute.triggerRange)
        {
            case Trigger.TriggerRange.SingleCell:
                TriggerDirection(inventoryItem.GetShape().itemDirection, 1);
                break;
            case Trigger.TriggerRange.DoubleCell:
                TriggerDirection(inventoryItem.GetShape().itemDirection, 2);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddTo_triggerItems(HashSet<Item> items)
    {
        foreach (var item in items)
        {
            if (triggerItems.ContainsKey(item.GetItemType()) == false)
                triggerItems.Add(item.GetItemType(), new List<object>());
            triggerItems[item.GetItemType()].Add(item.GetItemAttribute());
        }
    }

    private HashSet<Item> TriggerDirection(InventoryItem.Direction direction, int cellCount)
    {
        InventoryManager.GridPos basePos = inventoryItem.currentLayOnGrid[0].gridPos;
        HashSet<Item> ContainItems = new();
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
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && !ContainItems.Contains(tmp))
                        ContainItems.Add(tmp);
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
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && !ContainItems.Contains(tmp))
                        ContainItems.Add(tmp);
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
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && !ContainItems.Contains(tmp))
                        ContainItems.Add(tmp);
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
                    if (tmp != null && tmp.GetItemType() != Item.ItemType.None && !ContainItems.Contains(tmp))
                        ContainItems.Add(tmp);
                    basePos.gridX++;
                    cellCount--;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        AddTo_triggerItems(ContainItems);
        return null;
    }
}
