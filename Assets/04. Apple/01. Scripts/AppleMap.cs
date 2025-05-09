using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppleMap : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private AppleCell[,] _cells;

    public AppleCell this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return null;
            }

            return _cells[x, y];
        }
    }
    private int _width;
    private int _height;
    [SerializeField] private GridLayoutGroup _gridLayoutGroup;

    [SerializeField] private ContentSizeFitter _contentSizeFitter;

    [ContextMenu("MakeMapTest")]
    public void MakeMap()
    {
        ClearMap();

        _width = 30;
        _height = 20;
        
        _cells = new AppleCell[_width, _height];
        
        _contentSizeFitter.enabled = true;
        _gridLayoutGroup.enabled = true;

        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _width;
        
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var cell = AppleResourceManager.instance.GetUnusedCell();
                if (cell == null)
                {
                    Debug.LogError("Cell is null");
                    ClearMap();
                    return;
                }
                
                cell.Init(x, y);
                cell.transform.SetParent(_gridLayoutGroup.transform);
                _cells[x, y] = cell;
            }
        }
    }

    private void ClearMap()
    {
        if (_cells == null) return;
        foreach (var cell in _cells)
        {
            AppleResourceManager.instance.ReturnCell(cell);
        }
        _cells = null;
    }
    
    void OnEnable()
    {
        Canvas.willRenderCanvases += OnAfterLayout;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= OnAfterLayout;
    }

    void OnAfterLayout()
    {
        _contentSizeFitter.enabled = false;
        _gridLayoutGroup.enabled = false;
    }
    
    public GraphicRaycaster raycaster;
    public AppleCell startCell;
    public void OnPointerDown(PointerEventData eventData)
    {
        Clear();
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);
        results.ForEach(_ =>
        {
            var cell = _.gameObject.GetComponent<AppleCell>();
            if (cell != null)
            {
                startCell = cell;
                Select(cell);
            }
        });
    }

    public AppleCell endCell;
    public List<AppleCell> selectedCells = new List<AppleCell>();
    public void OnDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);
        results.ForEach(_ =>
        {
            var cell = _.gameObject.GetComponent<AppleCell>();
            if (cell != null)
            {
                if (endCell != cell)
                {
                    Select(cell);
                }
            }
        });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        int t = 0;
        foreach (var selectedCell in selectedCells)
        {
            t += selectedCell.number;
        }
        
        if (t == 10)
        {
            foreach (var selectedCell in selectedCells)
            {
                selectedCell.isActive = false;
                selectedCell.gameObject.SetActive(false);
            }
        }
        
        Clear();
    }
    
    public void Clear()
    {
        startCell = null;
        endCell = null;
        foreach (var selectedCell in selectedCells)
        {
            selectedCell.ActiveBorder(false);
        }
        selectedCells.Clear();
    }

    public void Select(AppleCell cell)
    {
        endCell = cell;
        foreach (var selectedCell in selectedCells)
        {
            selectedCell.ActiveBorder(false);
        }
        selectedCells.Clear();
                    
        for (int x = Mathf.Min(startCell.x, endCell.x); x <= Mathf.Max(startCell.x, endCell.x); x++)
        {
            for (int y = Mathf.Min(startCell.y, endCell.y); y <= Mathf.Max(startCell.y, endCell.y); y++)
            {
                if (this[x, y].isActive)
                {
                    this[x, y].ActiveBorder(true);
                    selectedCells.Add(this[x, y]);
                }
            }
        }
    }
}
