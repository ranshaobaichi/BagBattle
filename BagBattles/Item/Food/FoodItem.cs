using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Item
{
    FoodItemAttribute foodItemAttributes = new();
    public FoodItem(FoodItemAttribute foodItemAttribute)
    {
        foodItemAttributes = foodItemAttribute;
    }

    public override void UseItem()
    {
        Debug.Log("食物道具使用");
        foreach (var foodItemAttribute in foodItemAttributes.foodItemAttributes)
        {
            switch (foodItemAttribute.foodDurationType)
            {
                case Food.FoodDurationType.None:
                    Debug.LogError("食物道具持续时间未设置");
                    break;
                case Food.FoodDurationType.Permanent:
                    switch (foodItemAttribute.foodBonusType)
                    {
                        case Food.FoodBonusType.AttackByValue:
                            BulletSpawner.Instance.AddPermanentAddDamage(foodItemAttribute.foodBonusValue);
                            Debug.Log($"食物道具永久增加伤害: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.AttackByPercent:
                            BulletSpawner.Instance.AddPermanentPercentageDamage(foodItemAttribute.foodBonusValue);
                            Debug.Log($"食物道具永久增加百分比伤害: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.Speed:
                            PlayerController.Instance.AddPermanentSpeed(foodItemAttribute.foodBonusValue);
                            Debug.Log($"食物道具永久增加速度: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.Health:
                            Debug.LogError("食物道具永久增加生命值未实现");
                            Debug.Log($"食物道具永久增加生命值: {foodItemAttribute.foodBonusValue}");
                            break;
                        default:
                            Debug.LogError($"未知的食物道具效果类型: {foodItemAttribute.foodBonusType}");
                            break;
                    }
                    break;
                case Food.FoodDurationType.Temporary:
                    Debug.Log("食物道具临时生效");
                    switch (foodItemAttribute.foodBonusType)
                    {
                        case Food.FoodBonusType.AttackByValue:
                            BulletSpawner.Instance.AddTemporaryAddDamage(foodItemAttribute.foodBonusValue, foodItemAttribute.roundLeft);
                            Debug.Log($"食物道具临时增加伤害: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.AttackByPercent:
                            BulletSpawner.Instance.AddTemporaryPercentageDamage(foodItemAttribute.foodBonusValue, foodItemAttribute.roundLeft);
                            Debug.Log($"食物道具临时增加百分比伤害: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.Speed:
                            PlayerController.Instance.AddTemporarySpeed(foodItemAttribute.foodBonusValue, foodItemAttribute.roundLeft);
                            Debug.Log($"食物道具临时增加速度: {foodItemAttribute.foodBonusValue}");
                            break;
                        case Food.FoodBonusType.Health:
                            Debug.LogError("食物道具临时增加生命值未实现");
                            Debug.Log($"食物道具临时增加生命值: {foodItemAttribute.foodBonusValue}");
                            break;
                        default:
                            Debug.LogError($"未知的食物道具效果类型: {foodItemAttribute.foodBonusType}");
                            break;
                    }
                    break;
                default:
                    Debug.LogError($"未知的食物持续时间类型: {foodItemAttribute.foodDurationType}");
                    break;
            }
        }
        InventoryManager.Instance.RemoveFoodItem(foodItemAttributes.foodInventoryItem); // 使用道具后移除道具
    }
}