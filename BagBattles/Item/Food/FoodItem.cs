using UnityEngine;
using Assets.BagBattles.Types;

public class FoodItem : Item
{
    public FoodItemAttribute foodItemAttributes = new();
    public FoodItem(FoodType foodType)
    {
        if (ItemAttribute.Instance.GetAttribute(Item.ItemType.FoodItem, foodType) is FoodItemAttribute attr)
        {
            foodItemAttributes = attr;
            itemType = Item.ItemType.FoodItem;
        }
        else
        {
            Debug.LogError("无法获取食物道具属性");
        }
    }
    public override object GetSpecificItemType() => foodItemAttributes.specificFoodType;
    public override void UseItem()
    {
        Debug.Log("食物道具使用");
        foreach (var foodItemAttribute in foodItemAttributes.foodItemAttributes)
        {
            if (foodItemAttribute.foodBonusType == Food.FoodBonusType.None ||
                foodItemAttribute.foodDurationType == Food.FoodDurationType.None ||
                foodItemAttribute.foodBonusValue < 0
                )
            {
                Debug.LogError("食物道具属性错误");
                continue;
            }
            PlayerController.Instance.AddBonus(foodItemAttribute);
            Debug.Log($"Applied bonus: {foodItemAttribute.foodBonusValue} of type: {foodItemAttribute.foodBonusType}");
        }
        if (!(foodItemAttributes.destroyCount == -1 && foodItemAttributes.destroyCount == 0))
            foodItemAttributes.destroyCount--;
        // InventoryManager.Instance.RemoveFoodItem(sourceInventoryItem as FoodInventoryItem);
    }
}