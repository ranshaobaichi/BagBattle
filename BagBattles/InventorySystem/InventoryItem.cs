using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

// BUG：添加basePoint（预制体中），用来规定物体放置中心点
public abstract class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    protected Vector3 originalPosition;
    protected InventoryManager inventoryManager;
    protected GameObject inventorySystem;
    [NonSerialized] protected List<GridCell> currentLayOnGrid;   //在哪个块上
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
    [HideInInspector] public bool triggerDectectFlag = false; // 可否被触发器检测标志位
    #endregion

    #region 对外接口
    public Item.ItemType GetItemType() => itemType;
    public ItemShape GetShape() => itemShape;
    public Direction GetDirection() => itemDirection;
    public void LayOnGrid(GridCell gridCell) => currentLayOnGrid.Add(gridCell);
    /// <summary>
    /// 初始化物品 注意设置方向与形状
    /// </summary>
    public abstract bool Initialize(object type); // 初始化物品
    public abstract object GetSpecificType();
    #endregion

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
        // 初始化变量
        currentLayOnGrid = new();
        previousLayOnGrid = new();
        raycastPoints = new();
        previousHoveredCells = new();
        itemDirection = Direction.UP; // 默认朝上

        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        inventorySystem = GameObject.FindGameObjectWithTag("InventorySystem");
        canvas = gameObject.AddComponent<Canvas>();
        gameObject.AddComponent<GraphicRaycaster>();

        // 设置Canvas属性
        canvas.overrideSorting = true; // 允许覆盖排序
        canvas.sortingOrder = 1; // 设置排序层级


        // 这里可以根据需要初始化物品的名称和图标
        // SetItemDetails(itemType);
        if (raycastPoints == null)
            Debug.LogError("raycastPoints is null");
        foreach (Transform child in transform)
        {
            if (child.CompareTag("RaycastPoint"))
                raycastPoints.Add(child.GetComponent<RectTransform>());
        }

        // 承载的道具初始化
        triggerDectectFlag = true;
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

        //还原原所在格子状态
        foreach (GridCell gridCell in currentLayOnGrid)
        {
            gridCell.SetCanPlaceItem(true);
            gridCell.itemOnGrid = null; // 恢复格子物品类型
        }
        previousHoveredCells.Clear(); // 清空上次检测的格子
        previousLayOnGrid.Clear(); // 清空上次检测的格子
        foreach (GridCell gridCell in currentLayOnGrid)
            previousLayOnGrid.Add(gridCell);
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

        List<InventoryManager.GridPos> target = new List<InventoryManager.GridPos>();
        var ret = inventoryManager.TryPlaceItemInGrid(this, previousHoveredCells, target);
    TRYAGAIN:
        switch (ret)
        {
            case -2:
                Debug.LogError("放置target未初始化，尝试重新放置");
                target.Clear();
                target = new List<InventoryManager.GridPos>();
                ret = inventoryManager.TryPlaceItemInGrid(this, previousHoveredCells, target);
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
                }
                break;
            default:
                Debug.LogError("放置产生未知错误");
                return;
        }
        previousParent = null; // 清空上次放置的父物体
        previousLayOnGrid.Clear(); // 清空上次占据的格子
        foreach (var cell in previousHoveredCells)
            cell.SetNormal();
        previousHoveredCells.Clear();
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
