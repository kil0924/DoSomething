using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Match3Map : MonoBehaviour
{
    public Match3Cell this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return null;
            }
            return cells[x, y];
        }
    }
    public Match3Cell[,] cells;
    private int _width;
    private int _height;
    public int width => _width;
    public int height => _height;
    
    [SerializeField]
    private GridLayoutGroup _gridLayoutGroup;
    
    [SerializeField]
    private ContentSizeFitter _contentSizeFitter;
    
    public void MakeMap(int width, int height)
    {
        ClearMap();
        
        _width = width;
        _height = height;
        
        cells = new Match3Cell[width, height];

        _contentSizeFitter.enabled = true;
        _gridLayoutGroup.enabled = true;
        
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _width;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)    
            {
                var cell = Match3ResourceManager.instance.GetUnusedCell();
                if (cell == null)
                {
                    Debug.LogError("Cell is null");
                    ClearMap();
                    return;
                }
                cell.Init(x, y);
                cell.transform.SetParent(_gridLayoutGroup.transform);
                cell.SetOnDrop(OnDrop);
                cells[x, y] = cell;
            }
        }
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
    
    public int widthTest = 5;
    public int heightTest = 5;
    [ContextMenu( "MakeMapTest" )]
    public void MakeMapTest()
    {   
        MakeMap(widthTest, heightTest);
    }

    public void ClearMap()
    {
        if (cells == null) return;
        foreach (var cell in cells)
        {
            Match3ResourceManager.instance.ReturnCell(cell);
        }
        cells = null;
    }

    private Action<int, int> onClick;
    public void SetOnClick(Action<int, int> onClick)
    {
        this.onClick = onClick;
    }
    private void OnClick(int x, int y)
    {
        onClick?.Invoke(x, y);
    }
    
    private Action<int, int, int, int> onDrop;
    public void SetOnDrop(Action<int, int, int, int> onDrop)
    {
        this.onDrop = onDrop;
    }
    private void OnDrop(int x, int y, int dirX, int dirY)
    {
        onDrop?.Invoke(x, y, dirX, dirY);
    }
}
