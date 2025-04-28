using UnityEngine;
using Assets.BagBattles.Types;
using System.Linq;

public class OtherItem : Item
{
    [Header("其他类型类道具")]
    public OtherItemAttribute otherAttribute;

    public OtherItem(OtherType otherType)
    {
        if (ItemAttribute.Instance.GetAttribute(Item.ItemType.OtherItem, otherType) is OtherItemAttribute attr)
        {
            otherAttribute = attr;
            itemType = ItemType.OtherItem;
        }
        else
        {
            Debug.LogError("无法获取环绕物道具属性");
        }
    }
    public override object GetSpecificItemType() => otherAttribute.specificOtherType;
    public override void UseItem()
    {
        switch (otherAttribute.specificOtherType)
        {
            case OtherType.FireZone:
                ObjectPool.Instance.GetObject(otherAttribute.otherItemPrefab);
                break;
            case OtherType.D100:
                D100.Triggered(Random.Range(0, 100));
                break;
            case OtherType.FallingPillar:
                Vector2 spawnPosition = PlayerController.Instance.transform.position;
                spawnPosition += (Vector2)PlayerController.Instance.transform.right * 2f;
                GameObject pillar = ObjectPool.Instance.GetObject(otherAttribute.otherItemPrefab);
                pillar.transform.position = spawnPosition;
                break;
            case OtherType.NextShootDamageUp1:
            case OtherType.NextShootDamageUp2:
            case OtherType.NextShootDamageUp3:
                BulletSpawner.Instance.NextShootDamageUpAdd(float.Parse(otherAttribute.specificOtherType.ToString().Last().ToString()));
                break;
        }
    }
}
