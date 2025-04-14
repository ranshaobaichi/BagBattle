using UnityEngine;
using UnityEngine.UI;

public class Armor : MonoBehaviour {
    [Header("状态贴图")]
    public Sprite[] armor;
 
    private Image image;
    public int currentArmor;    // 当前护甲还可抵挡伤害次数
    private int maxArmor;   // 护甲最大可抵挡伤害次数
 
    void Awake()
    {
        image = GetComponent<Image>();
        image.preserveAspect = true;
    }

    public void Initialize(int capacity, int current = -1)
    {
        if(capacity != -1)
            maxArmor = capacity;
        if(current == -1)
            currentArmor = maxArmor;
        else
            currentArmor = current;
        UpdateDisplay();
    }
 
    public int ReduceArmor()
    {
        currentArmor--;
        if(currentArmor > 0)
            UpdateDisplay();
        return currentArmor;
    }

    public void ResetArmor()
    {
        currentArmor = maxArmor;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (image == null)
            image = GetComponent<Image>();
        image.sprite  = currentArmor switch
        {
            0 => armor[0],
            1 => armor[1],
            2 => armor[2],
            _ => null
        };
    }

    public bool IsEmpty => currentArmor <= 0;
    public void OnDestroy()
    {

    }
}