using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    private Image cellImage;
    private bool canPlaceItem = true;  // 用于判断该格子是否可以放置物品
    public InventoryManager.GridPos gridPos;
    private Canvas canvas;
    public InventoryItem itemOnGrid; // 格子上放置的物品类型
    public Transform centerContainer;

    void Awake()
    {
        cellImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(InventoryManager.GridPos p)
    {
        gridPos = new InventoryManager.GridPos(p.gridX, p.gridY);
        SetNormal();
        itemOnGrid = null; // 清空格子上的物品类型
    }

    public InventoryManager.GridPos GetPos() => gridPos;
    public bool GetCanPlace() => canPlaceItem;
    public void SetCanPlaceItem(bool value)
    {
        canPlaceItem = value;
        SetNormal();
    }
    public void SetNormal()
    {
        cellImage.color = Color.white;
        canvas.sortingOrder = 0; // 恢复格子排序
    }
    public void SetHighlight() => cellImage.color = new Color(0.5f, 1, 0.4f, 0.4f);
    public void SetInvalid()
    {
        canvas.sortingOrder = 2; // 提升格子排序
        cellImage.color = new Color(1, 0, 0, 0.5f);
    }
}
