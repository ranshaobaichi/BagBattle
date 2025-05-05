using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [Header("提示框引用")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform attributesContainer;
    [SerializeField] private GameObject attributePrefab;
    [SerializeField] private GameObject attributeListPrefab;
    
    [Header("设置")]
    [SerializeField] private Vector2 offset = new Vector2(15, 15);
    [SerializeField] private float padding = 10f;
    
    private RectTransform panelRectTransform;
    private List<GameObject> spawnedAttributes = new List<GameObject>();
    
    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);
            
        panelRectTransform = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();
    }
    
    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // 更新位置跟随鼠标
            Vector2 position = Input.mousePosition;
            position += offset;
            
            // 确保提示框不超出屏幕边界
            float pivotX = position.x / Screen.width > 0.5f ? 1 : 0;
            float pivotY = position.y / Screen.height > 0.5f ? 1 : 0;
            
            panelRectTransform.pivot = new Vector2(pivotX, pivotY);
            tooltipPanel.transform.position = position;
        }
    }

    // 清理当前显示的属性
    private void ClearAttributes()
    {
        foreach(var obj in spawnedAttributes)
            Destroy(obj);
        spawnedAttributes.Clear();
    }
    
    // 显示子弹类型提示
    public void ShowBulletTooltip(BulletItemAttribute bulletAttr)
    {
        ClearAttributes();
        titleText.text = bulletAttr.specificBulletType.ToString();
        descriptionText.text = bulletAttr.description;

        // 添加子弹特有属性
        var bulletType = bulletAttr.bulletType switch
        {
            Bullet.SingleBulletType.Normal_Bullet => "普通子弹",
            Bullet.SingleBulletType.Spear_Bullet => "穿透子弹",
            Bullet.SingleBulletType.Rocket_Bullet => "火箭子弹",
            Bullet.SingleBulletType.Lightning_Bullet => "闪电子弹",
            Bullet.SingleBulletType.Fire_Bullet => "燃烧子弹",
            Bullet.SingleBulletType.Ice_Bullet => "减速子弹",
            Bullet.SingleBulletType.Swallow_Bullet => "吞噬子弹",
            Bullet.SingleBulletType.Bomb_Bullet => "炸弹子弹",
            Bullet.SingleBulletType.Jump_Bullet => "跳跃子弹",
            Bullet.SingleBulletType.Split_Bullet => "分裂子弹",
            _ => bulletAttr.bulletType.ToString()
        };
        AddAttribute("子弹类型", bulletType);
        AddAttribute("装载数量", bulletAttr.bulletCount.ToString());
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }
    
    // 显示食物类型提示
    public void ShowFoodTooltip(FoodItemAttribute foodAttr)
    {
        ClearAttributes();
        titleText.text = foodAttr.specificFoodType.ToString();
        descriptionText.text = foodAttr.description;
        
        // 添加食物效果列表
        AddFoodEffects(foodAttr.foodItemAttributes);
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }
    
    // 显示环绕物类型提示
    public void ShowSurroundTooltip(SurroundItemAttribute surroundAttr)
    {
        ClearAttributes();
        titleText.text = surroundAttr.specificSurroundType.ToString();
        descriptionText.text = surroundAttr.description;
        
        AddAttribute("环绕物类型", surroundAttr.summonedSurroundingType.ToString());
        AddAttribute("数量", surroundAttr.surroundingCount.ToString());
        AddAttribute("持续时间", surroundAttr.surroundingDuration.ToString() + "秒");
        AddAttribute("加速倍率", surroundAttr.surroundingSpeedPercent.ToString("P0"));
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }
    
    // 显示触发器提示
    public void ShowTimeTriggerTooltip(Trigger.TimeTriggerAttribute triggerAttr)
    {
        ClearAttributes();
        titleText.text = triggerAttr.timeTriggerType.ToString();
        descriptionText.text = triggerAttr.description;
        
        AddAttribute("触发时间", triggerAttr.triggerTime.ToString() + "秒");
        var triggerRange = triggerAttr.triggerRange switch
        {
            Trigger.TriggerRange.SingleCell => "单格",
            Trigger.TriggerRange.DoubleCell => "两格",
            Trigger.TriggerRange.TripleCell => "三格",
            Trigger.TriggerRange.FullRow => "整行",
            Trigger.TriggerRange.FourStraightSingleCell => "四格",
            Trigger.TriggerRange.NineGrid => "九宫格",
            Trigger.TriggerRange.Cross => "十字形",
            _ => triggerAttr.triggerRange.ToString()
        };
        AddAttribute("触发范围", triggerRange);
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }
    
    public void ShowFireTriggerTooltip(Trigger.FireCountTriggerAttribute triggerAttr)
    {
        ClearAttributes();
        titleText.text = triggerAttr.fireTriggerType.ToString();
        descriptionText.text = triggerAttr.description;
        
        AddAttribute("触发开火次数", triggerAttr.fireCount.ToString() + "次");

        var triggerRange = triggerAttr.triggerRange switch
        {
            Trigger.TriggerRange.SingleCell => "单格",
            Trigger.TriggerRange.DoubleCell => "两格",
            Trigger.TriggerRange.TripleCell => "三格",
            Trigger.TriggerRange.FullRow => "整行",
            Trigger.TriggerRange.FourStraightSingleCell => "四格",
            Trigger.TriggerRange.NineGrid => "九宫格",
            Trigger.TriggerRange.Cross => "十字形",
            _ => triggerAttr.triggerRange.ToString()
        };
        AddAttribute("触发范围", triggerRange);
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }
    
    public void ShowByOtherTriggerTooltip(Trigger.ByOtherTriggerAttribute triggerAttr)
    {
        ClearAttributes();
        titleText.text = triggerAttr.byOtherTriggerType.ToString();
        descriptionText.text = triggerAttr.description;
        
        AddAttribute("触发器类型", triggerAttr.byOtherTriggerType.ToString());
        var triggerRange = triggerAttr.triggerRange switch
        {
            Trigger.TriggerRange.SingleCell => "单格",
            Trigger.TriggerRange.DoubleCell => "两格",
            Trigger.TriggerRange.TripleCell => "三格",
            Trigger.TriggerRange.FullRow => "整行",
            Trigger.TriggerRange.FourStraightSingleCell => "四格",
            Trigger.TriggerRange.NineGrid => "九宫格",
            Trigger.TriggerRange.Cross => "十字形",
            _ => triggerAttr.triggerRange.ToString()
        };
        AddAttribute("触发范围", triggerRange);
        
        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }

    public void ShowOtherTooltip(OtherItemAttribute otherAttr)
    {
        ClearAttributes();
        titleText.text = otherAttr.specificOtherType.ToString();
        descriptionText.text = otherAttr.description;

        tooltipPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }

    // 辅助方法：添加单个属性
    private void AddAttribute(string name, string value)
    {
        GameObject attrObject = Instantiate(attributePrefab, attributesContainer);
        spawnedAttributes.Add(attrObject);

        TextMeshProUGUI[] texts = attrObject.GetComponentsInChildren<TextMeshProUGUI>();
        texts[0].text = name + ":";
        texts[1].text = value;
    }
    
    // 辅助方法：添加食物效果列表
    private void AddFoodEffects(List<FoodItemAttribute.BasicFoodAttribute> effects)
    {
        GameObject listObj = Instantiate(attributeListPrefab, attributesContainer);
        spawnedAttributes.Add(listObj);
        
        Transform listContainer = listObj.transform.Find("EffectsContainer");
        TextMeshProUGUI headerText = listObj.GetComponentInChildren<TextMeshProUGUI>();
        headerText.text = "效果列表:";
        
        foreach (var effect in effects)
        {
            GameObject effectItem = Instantiate(attributePrefab, listContainer);
            string durationText = "";
            
            switch (effect.foodDurationType)
            {
                case Food.FoodDurationType.Permanent:
                    durationText = "永久";
                    break;
                case Food.FoodDurationType.TemporaryRounds:
                    durationText = effect.timeLeft + "回合";
                    break;
                case Food.FoodDurationType.TemporaryTime:
                    durationText = effect.timeLeft + "秒";
                    break;
            }
            
            TextMeshProUGUI[] texts = effectItem.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = GetFoodBonusTypeName(effect.foodBonusType);
            texts[1].text = effect.foodBonusValue.ToString("+0.##;-0.##") + " (" + durationText + ")";
        }
    }
    
    // 辅助方法：获取加成类型的友好名称
    private string GetFoodBonusTypeName(Food.FoodBonusType type)
    {
        switch (type)
        {
            case Food.FoodBonusType.AttackDamageByValue: return "攻击伤害";
            case Food.FoodBonusType.AttackDamageByPercent: return "攻击百分比";
            case Food.FoodBonusType.AttackSpeed: return "攻击速度";
            case Food.FoodBonusType.AttackRange: return "攻击范围";
            case Food.FoodBonusType.LoadSpeed: return "装填速度";
            case Food.FoodBonusType.Speed: return "移动速度";
            case Food.FoodBonusType.HealthUp: return "生命值上限";
            case Food.FoodBonusType.HealthRecover: return "生命值恢复";
            case Food.FoodBonusType.HealthDown: return "生命值减少";
            case Food.FoodBonusType.ArmorUp: return "护甲值提升";
            default: return type.ToString();
        }
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}