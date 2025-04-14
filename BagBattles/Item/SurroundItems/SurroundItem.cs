using UnityEngine;
using Assets.BagBattles.Types;

public class SurroundItem : Item
{
    [Header("子弹类道具")]
    public SurroundItemAttribute surroundAttribute;
    private Surrounding surrounding;    // 实例化的环绕物

    public SurroundItem(SurroundType surroundType)
    {
        if (ItemAttribute.Instance.GetAttribute(Item.ItemType.SurroundItem, surroundType) is SurroundItemAttribute attr)
        {
            surroundAttribute = attr;
            itemType = ItemType.SurroundItem;
        }
        else
        {
            Debug.LogError("无法获取环绕物道具属性");
        }
        surrounding = null;
    }
    public override object GetSpecificItemType() => surroundAttribute.specificSurroundType;
    public override void UseItem()
    {
        if (surrounding == null)
        {
            surrounding = ObjectPool.Instance.GetObject(surroundAttribute.surroundingPrefab).GetComponent<Surrounding>();
        }
        else
        {
            surrounding.SpeedUp(surroundAttribute.surroundingSpeedPercent, surroundAttribute.surroundingDuration);
        }
    }
}
