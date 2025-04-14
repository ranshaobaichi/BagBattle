using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using Unity.Burst.Intrinsics;
using System.Linq;
using System.IO;


[System.Serializable]
public class HealthData
{
    [System.Serializable]
    public class HeartData
    {
        public int maxHearts;
        public int healthPerHeart;
        public List<int> specificHealth = new();
    }
    
    [System.Serializable]
    public class ArmorData
    {
        public int initArmors;
        public int cntPerArmor;
        public List<int> specificArmor = new();
        public List<Food.Bonus> temporaryArmorBonus = new();
    }
    
    public HeartData heartData = new();
    public ArmorData armorData = new();
}
public class HealthController : MonoBehaviour
{
    const string playerHealthSaveDataPath = "playerHealthData.json";
    public static HealthController Instance { get; set; }

    [Header("配置")]
    public GameObject heartPrefab;
    public GameObject armorePrefab;

    public Transform heartPanel;
    public Transform armorPanel;

    [Tooltip("最大心数")] public int maxHearts;
    [Tooltip("每颗心的血量")] public int healthPerHeart;
    private LinkedListNode<Heart> currentHeart;
    private LinkedList<Heart> Hearts = new();

    [Tooltip("初始护甲数")] public int initArmors;
    [Tooltip("每颗护甲的承伤次数")] public int cntPerArmor;
    private LinkedList<Armor> Armors = new();
    private LinkedListNode<Armor> currentArmor = null;
    private LinkedList<Food.Bonus> temporaryArmorBonus = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (this != Instance)
            Destroy(gameObject);
    }

    void Start()
    {
        if (PlayerPrefs.GetInt(PlayerPrefsKeys.NEW_GAME_KEY) == 1)
            PrewarmPool();
    }

    private void PrewarmPool()
    {
        HealthUp(maxHearts);
        currentHeart = Hearts.Last;
        ArmorUp(initArmors);
        currentArmor = Armors.Last;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Player TakeDamage: " + damage);

        if (currentArmor != null)
        {
            Debug.Log("Armor take the damage: " + damage);
            int armorDamage = currentArmor.Value.ReduceArmor();
            if (armorDamage <= 0)
            {
                // 先减少临时护甲
                if (temporaryArmorBonus.Count > 0)
                {
                    var bonus = temporaryArmorBonus.First;
                    var bonusValue = bonus.Value;
                    bonusValue.bonusValue -= 1;
                    temporaryArmorBonus.RemoveFirst();
                    if (bonusValue.bonusValue > 0)
                        temporaryArmorBonus.AddFirst(bonusValue);
                }

                // 删除护甲
                var tmp = currentArmor;
                currentArmor = currentArmor.Previous;
                Armors.RemoveLast();
                Destroy(tmp.Value.gameObject);
            }
            return;
        }

        int remainingDamage = damage;
        while (remainingDamage > 0)
        {
            //Debug.Log("damage");
            int actualDamage = currentHeart.Value.ReduceHealth(remainingDamage);
            remainingDamage -= actualDamage;

            if (currentHeart.Value.IsEmpty)
            {
                if (currentHeart.Previous != null)
                    currentHeart = currentHeart.Previous;
                else
                {
                    GameOver();
                    break;
                }
            }
        }

        if (currentHeart.Value.IsEmpty)
            GameOver();
    }

    private void GameOver()
    {
        PlayerController.Instance.Dead();
    }

    /// <summary>
    /// 增加血量
    /// </summary>
    /// <param name="value"> 血量增加颗数 </param>
    /// <param name="health"> 每颗血量增加数值 </param>
    public void HealthUp(int value, int[] health = null)
    {
        if (value <= 0) return;
        for (int i = 0; i < value; i++)
        {
            var heart = Instantiate(heartPrefab, heartPanel).GetComponent<Heart>();
            heart.gameObject.SetActive(true);
            heart.Initialize(healthPerHeart, health == null ? healthPerHeart : health[i]);
            heart.transform.SetAsFirstSibling();
            Hearts.AddFirst(heart);
        }
    }

    public void HealthDown(int value)
    {
        if (value <= 0) return;
        for (int i = 0; i < value; i++)
        {
            // 保留最后一颗心
            if (Hearts.Count > 1)
            {
                var heart = Hearts.Last.Value;
                Destroy(heart.gameObject);
                Hearts.RemoveLast();
            }
        }
        currentHeart = Hearts.Last;
        while (currentHeart != null && currentHeart.Value.IsEmpty)
            currentHeart = currentHeart.Previous;
        if (currentHeart == null)
        {
            Debug.LogError("No hearts left!");
            GameOver();
        }
    }

    public void ArmorUp(int value, int[] armors = null)
    {
        if (value <= 0) return;
        for (int i = 0; i < value; i++)
        {
            var armor = Instantiate(armorePrefab, armorPanel).GetComponent<Armor>();
            armor.gameObject.SetActive(true);
            armor.Initialize(armors == null ? cntPerArmor : armors[i]);
            armor.transform.SetAsFirstSibling();
            Armors.AddFirst(armor);
        }
        currentArmor = Armors.Last;
    }

    public void ArmorDown(int value)
    {
        if (value <= 0) return;
        for (int i = 0; i < value; i++)
        {
            // 保留最后一颗护甲
            if (Armors.Count > 0)
            {
                var armor = Armors.Last.Value;
                Destroy(armor.gameObject);
                Armors.RemoveLast();
            }
        }
        currentArmor = Armors.Last;
        while (currentArmor != null && currentArmor.Value.IsEmpty)
            currentArmor = currentArmor.Previous;
    }

    public void AddBonus(Food.FoodBonusType type, float value, Food.FoodDurationType foodDurationType, float rounds = 1)
    {
        switch (type)
        {
            case Food.FoodBonusType.HealthUp:
                if (foodDurationType == Food.FoodDurationType.Permanent)
                    HealthUp((int)value);
                else
                {
                    Debug.LogError("临时增加血量不支持");
                    return;
                }
                break;
            case Food.FoodBonusType.HealthDown:
                if (foodDurationType == Food.FoodDurationType.Permanent)
                    HealthDown((int)value);
                else
                {
                    Debug.LogError("临时减少血量不支持");
                    return;
                }
                break;
            case Food.FoodBonusType.ArmorUp:
                if (foodDurationType == Food.FoodDurationType.Permanent)
                {
                    ArmorUp((int)value);
                }
                else
                {
                    temporaryArmorBonus.AddLast(new Food.Bonus(value, rounds));
                    ArmorUp((int)value);
                }
                break;
            default:
                Debug.LogError($"加成类型{type}不支持");
                break;
        }
    }

    public void DecreaseTemporaryBonus()
    {
        // 清除临时加成
        ArmorDown((int)temporaryArmorBonus.DecreaseRounds());
    }

    private void OnDestroy()
    {

    }

    public void StoreHealthData()
    {
        HealthData healthData = new()
        {
            heartData = new()
            {
                maxHearts = maxHearts,
                healthPerHeart = healthPerHeart,
            },
            armorData = new()
            {
                initArmors = initArmors,
                cntPerArmor = cntPerArmor,
                temporaryArmorBonus = temporaryArmorBonus.ToList(),
            }
        };

        var healthNode = Hearts.Last;
        while (healthNode != null)
        {
            healthData.heartData.specificHealth.Add(healthNode.Value.currentHealth);
            healthNode = healthNode.Previous;
        }

        var armorNode = Armors.Last;
        while (armorNode != null)
        {
            healthData.armorData.specificArmor.Add(armorNode.Value.currentArmor);
            armorNode = armorNode.Previous;
        }

        string jsonData = JsonUtility.ToJson(healthData, true);
        string savePath = Path.Combine(Application.persistentDataPath, playerHealthSaveDataPath);
        File.WriteAllText(savePath, jsonData);
        Debug.Log($"Health data saved to {savePath}");
    }

    public void LoadHealthData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, playerHealthSaveDataPath);
        
        if (!File.Exists(filePath))
        {
            Debug.Log("没有找到血量数据存档，使用默认值");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        HealthData healthData = JsonUtility.FromJson<HealthData>(jsonData);

        // 清除当前所有心脏和护甲
        Hearts = new();
        Armors = new();
        temporaryArmorBonus = new();

        // 设置基本配置
        maxHearts = healthData.heartData.maxHearts;
        healthPerHeart = healthData.heartData.healthPerHeart;
        initArmors = healthData.armorData.initArmors;
        cntPerArmor = healthData.armorData.cntPerArmor;

        // 恢复生命
        HealthUp(maxHearts, healthData.heartData.specificHealth.ToArray());

        // 恢复护甲
        ArmorUp(healthData.armorData.specificArmor.Count, healthData.armorData.specificArmor.ToArray());

        // 恢复临时护甲加成
        foreach (var bonus in healthData.armorData.temporaryArmorBonus)
        {
            temporaryArmorBonus.AddLast(bonus);
        }

        // 更新当前生命和护甲引用
        currentHeart = Hearts.Last;
        while (currentHeart != null && currentHeart.Value.IsEmpty)
            currentHeart = currentHeart.Previous;
            
        currentArmor = Armors.Last;
        while (currentArmor != null && currentArmor.Value.IsEmpty)
            currentArmor = currentArmor.Previous;

        Debug.Log($"血量数据已从 {filePath} 加载");
    }
}