
using System.Collections.Generic;

public class FoodInventoryItem : InventoryItem
{
    public FoodItem.FoodItemAttribute foodItemAttributes = new();
    public FoodInventoryItem()
    {
        itemType = Item.ItemType.FoodItem;
        foodItemAttributes.foodInventoryItem = this;
    }
    public override object GetAttribute() => foodItemAttributes;
}