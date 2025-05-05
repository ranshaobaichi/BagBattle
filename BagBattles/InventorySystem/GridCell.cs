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
    public Sprite normalSprite; // 正常状态的图片
    public Sprite seletedSprite; // 选中状态的图片

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
        if (cellImage == null || canvas == null) return;
        cellImage.color = Color.white;
        cellImage.sprite = normalSprite;
        canvas.sortingOrder = 0; // 恢复格子排序
    }
    public void SetHighlight()
    {
        cellImage.color = new Color(0.5f, 1, 0.4f, 0.4f);
        cellImage.sprite = seletedSprite;
    }
    public void SetInvalid()
    {
        if (cellImage == null || canvas == null) return;
        canvas.sortingOrder = 2; // 提升格子排序
        cellImage.color = new Color(1, 0, 0, 0.5f);
        cellImage.sprite = normalSprite;
    }
}
