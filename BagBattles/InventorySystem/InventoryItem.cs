using System;
using System.Collections.Generic;
using Assets.BagBattles.Types;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

// BUG：添加basePoint（预制体中），用来规定物体放置中心点
public abstract class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    public enum Direction
    {
        NONE,
        UP,
        LEFT,
        DOWN,
        RIGHT
    }
    [Serializable]
    public enum ItemShape
    {
        NONE,
        SQUARE_11,
        RECT_12,
        RECT_13,
        L_12_11
    }

    #region 仓库物品基础属性
    [Header("物体基础属性")]
    // public string itemName;
    protected Sprite itemIcon = null;
    // public Item.ItemType itemType;
    protected ItemShape itemShape = ItemShape.NONE;
    protected Direction itemDirection = Direction.UP;
    public Guid inventoryID = Guid.NewGuid(); // 唯一ID

    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    protected Vector3 originalPosition;
    protected GameObject inventorySystem;
    [NonSerialized] protected List<GridCell> currentLayOnGrid = new();   //在哪个块上
    [NonSerialized] protected List<GridCell> previousLayOnGrid;
    protected Transform previousParent;

    //用于射线检测覆盖物体
    [SerializeField]
    [NonSerialized] protected List<RectTransform> raycastPoints;
    [NonSerialized] protected List<GridCell> previousHoveredCells;
    protected Canvas canvas;
    #endregion

    #region 道具属性
    [Tooltip("道具类型")] protected Item.ItemType itemType;
    public bool triggerDectectFlag; // 可否被触发器检测标志位
    protected string description;
    #endregion

    #region 接口
    public Item.ItemType GetItemType() => itemType;
    public ItemShape GetShape() => itemShape;
    public Direction GetDirection() => itemDirection;
    public List<GridCell> GetCurrentLayOnGrid() => currentLayOnGrid;
    public void LayOnGrid(GridCell gridCell) => currentLayOnGrid.Add(gridCell);
    /// <summary>
    /// 初始化物品 注意设置方向与形状
    /// </summary>
    public abstract bool Initialize(object type); // 初始化物品
    public abstract object GetSpecificType();
    public string GetDescription() => description; // 获取物品描述
    #endregion
    protected virtual void SetIcon()
    {
        if (itemIcon == null && !ItemIcon.Instance.TryGetIcon(itemType, (Enum)GetSpecificType(), false, out itemIcon))
        {
            Debug.LogError($"道具{gameObject.name}图标设置错误！");
            return;
        }
        transform.GetComponent<Image>().sprite = itemIcon;
    }

    protected void InitializeDirection(Direction direction)
    {
        if (direction == Direction.NONE)
        {
            Debug.LogError("物品方向初始化错误");
            return;
        }
        while (itemDirection != direction)
            RotateTransform();
    }

    [ContextMenu("Rotate")]
    public void RotateTransform()
    {
        // 物品形状顺时针旋转90度
        transform.Rotate(0f, 0f, -90f); // 顺时针旋转90度
        itemDirection = itemDirection switch
        {
            Direction.UP => Direction.RIGHT,
            Direction.RIGHT => Direction.DOWN,
            Direction.DOWN => Direction.LEFT,
            Direction.LEFT => Direction.UP,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected void Awake()
    {
        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventorySystem = GameObject.FindGameObjectWithTag("InventorySystem");
        canvas = gameObject.AddComponent<Canvas>();
        gameObject.AddComponent<GraphicRaycaster>();

        // 这里可以根据需要初始化物品的名称和图标
        // SetItemDetails(itemType);
    }

    void Start()
    {
        // 初始化变量
        previousLayOnGrid = new();
        raycastPoints = new();
        previousHoveredCells = new();

        // 获取射线检测点
        if (raycastPoints == null)
            Debug.LogError("raycastPoints is null");
        foreach (Transform child in transform)
        {
            if (child.CompareTag("RaycastPoint"))
                raycastPoints.Add(child.GetComponent<RectTransform>());
        }

        // 设置Canvas属性
        canvas.overrideSorting = true; // 允许覆盖排序
        canvas.sortingOrder = 1; // 设置排序层级

        // 可以被触发器检测标志
        triggerDectectFlag = true;

        // 设置图标
        SetIcon();
    }

    // 鼠标进入物品时显示提示
    // 在OnPointerEnter方法中实现
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            switch (itemType)
            {
                case Item.ItemType.BulletItem:
                    var bulletType = (BulletType)GetSpecificType();
                    if (ItemAttribute.Instance.GetAttribute(itemType, bulletType) is BulletItemAttribute bulletAttr)
                        TooltipManager.Instance.ShowBulletTooltip(bulletAttr);
                    break;

                case Item.ItemType.FoodItem:
                    var foodType = (FoodType)GetSpecificType();
                    if (ItemAttribute.Instance.GetAttribute(itemType, foodType) is FoodItemAttribute foodAttr)
                        TooltipManager.Instance.ShowFoodTooltip(foodAttr);
                    break;

                case Item.ItemType.SurroundItem:
                    var surroundType = (SurroundType)GetSpecificType();
                    if (ItemAttribute.Instance.GetAttribute(itemType, surroundType) is SurroundItemAttribute surroundAttr)
                        TooltipManager.Instance.ShowSurroundTooltip(surroundAttr);
                    break;

                case Item.ItemType.TriggerItem:
                    if (this is TriggerInventoryItem triggerInventoryItem)
                    {
                        Trigger.TriggerType triggertype = triggerInventoryItem.GetTriggerType();
                        switch (triggertype)
                        {
                            case Trigger.TriggerType.ByFireTimes:
                                var fireTriggerType = (FireTriggerType)GetSpecificType();
                                if (ItemAttribute.Instance.GetAttribute(itemType, fireTriggerType) is Trigger.FireCountTriggerAttribute fireAttr)
                                    TooltipManager.Instance.ShowFireTriggerTooltip(fireAttr);
                                break;

                            case Trigger.TriggerType.ByTime:
                                var timeTriggerType = (TimeTriggerType)GetSpecificType();
                                if (ItemAttribute.Instance.GetAttribute(Item.ItemType.TriggerItem, triggertype, timeTriggerType) is Trigger.TimeTriggerAttribute timeAttr)
                                    TooltipManager.Instance.ShowTimeTriggerTooltip(timeAttr);
                                break;
                            case Trigger.TriggerType.ByOtherTrigger:
                                var byOtherTriggerType = (ByOtherTriggerType)GetSpecificType();
                                if (ItemAttribute.Instance.GetAttribute(Item.ItemType.TriggerItem, triggertype, byOtherTriggerType) is Trigger.ByOtherTriggerAttribute byOtherAttr)
                                    TooltipManager.Instance.ShowByOtherTriggerTooltip(byOtherAttr);
                                break;
                        }
                    }
                    break;

                case Item.ItemType.OtherItem:
                    var otherType = (OtherType)GetSpecificType();
                    if (ItemAttribute.Instance.GetAttribute(itemType, otherType) is OtherItemAttribute otherAttr)
                        TooltipManager.Instance.ShowOtherTooltip(otherAttr);
                    break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (triggerDectectFlag && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    // 开始拖动
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        canvasGroup.alpha = 0.6f; // 拖动时物品透明
        canvasGroup.blocksRaycasts = false; // 禁止物品在拖动时与其他物品交互
        previousParent = transform.parent; // 记录原父物体
        transform.SetParent(inventorySystem.transform); // 设置物品的父物体为InventorySystem
        rectTransform.SetAsLastSibling(); // 设置物品在父物体中的层级为最后一个
        previousLayOnGrid.Clear(); // 清空上次占据的格子

        previousLayOnGrid.AddRange(currentLayOnGrid); // 记录上次占据的格子
        //还原原所在格子状态
        foreach (GridCell gridCell in currentLayOnGrid)
        {
            Debug.Log("还原格子状态: " + gridCell.gridPos.gridX + " " + gridCell.gridPos.gridY);
            gridCell.SetCanPlaceItem(true);
            gridCell.itemOnGrid = null; // 恢复格子物品类型
        }
        previousHoveredCells.Clear(); // 清空上次检测的格子
        currentLayOnGrid.Clear(); // 清空当前格子
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;

        // 恢复上次检测的格子为正常颜色
        foreach (var cell in previousHoveredCells)
            cell.SetNormal();

        // 旋转
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Rotate");
            RotateTransform();
        }

        List<GridCell> hoveredCells = GetHoveredCells();  //新检测
        foreach (var cell in hoveredCells)
        {
            if (cell.GetCanPlace())
                cell.SetHighlight();
            else
                cell.SetInvalid();
        }

        previousHoveredCells.Clear();
        previousHoveredCells = hoveredCells;
    }

    // 拖动结束
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // 恢复物品透明度
        canvasGroup.blocksRaycasts = true; // 恢复物品的交互
        TryPlaceItem(previousHoveredCells); // 尝试放置物品
        previousParent = null; // 清空上次放置的父物体
        previousLayOnGrid.Clear(); // 清空上次占据的格子
        foreach (var cell in previousHoveredCells)
            cell.SetNormal();
        previousHoveredCells.Clear();
    }

    public void TryPlaceItem(List<InventoryManager.GridPos> gridPoses)
    {
        List<GridCell> targetCells = new List<GridCell>();
        Debug.Log("位置数量: " + gridPoses.Count);
        foreach (var gridPos in gridPoses)
        {
            if (InventoryManager.Instance.gridCells.ContainsKey(gridPos))
                targetCells.Add(InventoryManager.Instance.gridCells[gridPos]);
            else
                Debug.LogError($"({gridPos.gridX},{gridPos.gridY})位置格子不存在");
        }
        TryPlaceItem(targetCells);
    }
    public void TryPlaceItem(List<GridCell> targetCells)
    {
        List<InventoryManager.GridPos> target = new List<InventoryManager.GridPos>();
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        var ret = InventoryManager.Instance.TryPlaceItemInGrid(this, targetCells, target);
    TRYAGAIN:
        switch (ret)
        {
            case -2:
                Debug.LogError("放置target未初始化，尝试重新放置");
                target.Clear();
                target = new List<InventoryManager.GridPos>();
                ret = InventoryManager.Instance.TryPlaceItemInGrid(this, targetCells, target);
                goto TRYAGAIN;
            case -1:
                Debug.Log("放置失败，放到空白区域");
                currentLayOnGrid.Clear();
                break;
            case 0:
                Debug.Log("放置失败，放到已被占据的格子上");
                transform.SetParent(previousParent); // 恢复物品的父物体
                rectTransform.anchoredPosition = Vector2.zero; // 恢复物品的位置
                rectTransform.position = originalPosition;
                //原位置回放
                foreach (GridCell gridCell in previousLayOnGrid)
                {
                    currentLayOnGrid.Add(gridCell);
                    gridCell.SetCanPlaceItem(false);
                    gridCell.itemOnGrid = this; // 恢复格子物品类型
                }
                break;
            case 1:
                Debug.Log("放置成功");
                rectTransform.localScale = Vector3.one; // 恢复物品缩放
                if (target.Count == 0)
                {
                    Debug.LogError("放置成功，但target为空");
                    return;
                }
                foreach (var cell in target)
                {
                    Debug.Log("cell pos: " + cell.gridX + " " + cell.gridY);
                    // Debug.Log("cell pos: " + cell.gridX + " " + cell.gridY);
                    InventoryManager.Instance.gridCells[cell].SetCanPlaceItem(false);
                    currentLayOnGrid.Add(InventoryManager.Instance.gridCells[cell]);
                    InventoryManager.Instance.gridCells[cell].itemOnGrid = this; // 恢复格子物品类型
                    Debug.Log("物品占据格子数量: " + currentLayOnGrid.Count);
                }
                break;
            default:
                Debug.LogError("放置产生未知错误");
                return;
        }
    }

    // 封装方法，获取当前所有子物体射线位置的GridCell
    protected List<GridCell> GetHoveredCells()
    {
        List<GridCell> hoveredCells = new();

        foreach (var point in raycastPoints)
        {
            Vector3 screenPosition = point.position;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                GridCell cell = result.gameObject.GetComponent<GridCell>();
                if (cell && !hoveredCells.Contains(cell))
                    hoveredCells.Add(cell);
            }
        }

        return hoveredCells;
    }

}
