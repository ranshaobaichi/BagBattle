using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionButton : MonoBehaviour
{
    public GameObject optionUI; // 选项菜单对象
    public bool previousInventoryUIActive;
    public void ShowOptionUI()
    {
        TimeController.Instance.PauseGame();
        optionUI.SetActive(true);
        previousInventoryUIActive = InventorySystem.Instance.isActiveAndEnabled;
        InventorySystem.Instance.gameObject.SetActive(false); // 隐藏物品栏
        gameObject.SetActive(false); // 隐藏当前按钮
    }
}
