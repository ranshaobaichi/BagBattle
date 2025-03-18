using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    public enum Direction
    {
        UP,
        LEFT,
        DOWN,
        RIGHT
    }
    [System.Serializable]
    public enum ItemShape
    {
        SQUARE_11,
        RECT_12,
        RECT_13,
        L_SHAPE_12_11
    }
    [System.Serializable]
    public struct Shape
    {
        public ItemShape itemShape;
        public Direction itemDirection;
    }

    [Header("物体基础属性")]
    // public string itemName;
    protected Sprite itemIcon;
    // public Item.ItemType itemType;
    public Shape shape;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private InventoryManager inventoryManager;
    private GameObject inventorySystem;
    [SerializeField]
    public List<GridCell> currentLayOnGrid = new();   //在哪个块上
    [SerializeField]
    private List<GridCell> previousLayOnGrid = new();
    private Transform previousParent;

    //用于射线检测覆盖物体
    private List<RectTransform> raycastPoints = new();
    private List<GridCell> previousHoveredCells = new();
    private Canvas canvas;

    public Item item; // 物品脚本

    public void LayOnGrid(GridCell gridCell) => currentLayOnGrid.Add(gridCell);
    public Shape GetShape() => shape;
    
    [ContextMenu("Rotate")]
    public void RotateTransform()
    {
        // 物品形状顺时针旋转90度
        transform.Rotate(0f, 0f, 90f); // 顺时针旋转90度
        shape.itemDirection = (Direction)(((int)shape.itemDirection + 1) % 4); // 顺时针旋转90度
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        inventorySystem = GameObject.FindGameObjectWithTag("InventorySystem");
        canvas = gameObject.AddComponent<Canvas>();
        gameObject.AddComponent<GraphicRaycaster>();
        canvas.overrideSorting = true; // 允许覆盖排序
        canvas.sortingOrder = 1; // 设置排序层级
    }

    void OnEnable()
    {
        if(item == null)
            item = GetComponent<Item>();
        if (item == null)
        {
            Debug.LogError("物品脚本未找到，请确保物体上有Item组件");
            return;
        }
        // 这里可以根据需要初始化物品的名称和图标
        // SetItemDetails(itemType);
        foreach (Transform child in transform)
        {
            if (child.CompareTag("RaycastPoint"))
                raycastPoints.Add(child.GetComponent<RectTransform>());
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
                    gridCell.itemOnGrid = item; // 恢复格子物品类型
                }
                break;
            case 1:
                Debug.Log("放置成功");
                if(target.Count == 0)
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
                    InventoryManager.Instance.gridCells[cell].itemOnGrid = item; // 恢复格子物品类型
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
    private List<GridCell> GetHoveredCells()
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
