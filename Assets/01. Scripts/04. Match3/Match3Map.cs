using UnityEngine;
using UnityEngine.UI;

public class Match3Map : MonoBehaviour
{
    public Match3Cell[,] cells;
    private int _width;
    private int _height;
    public int width => width;
    public int height => height;
    
    [SerializeField]
    private GridLayoutGroup _gridLayoutGroup;
    
    public void MakeMap(int width, int height)
    {
        ClearMap();
        
        _width = width;
        _height = height;
        
        cells = new Match3Cell[width, height];
        
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _width;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
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
                cells[x, y] = cell;
            }
        }
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
}
